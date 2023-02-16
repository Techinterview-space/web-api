using System;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Files;
using Domain.Services.Organizations;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Controllers.Files;

[ApiController]
[Route("api/candidate-cv")]
[HasAnyRole]
public class CandidateCvController : ControllerBase
{
    private readonly ICvStorage _storage;
    private readonly DatabaseContext _context;
    private readonly IAuthorization _auth;

    public CandidateCvController(
        ICvStorage storage,
        DatabaseContext context,
        IAuthorization auth)
    {
        _storage = storage;
        _context = context;
        _auth = auth;
    }

    [HttpPost("{cardId:guid}/upload")]
    public async Task<IActionResult> UploadAsync(
        [FromRoute] Guid cardId,
        [FromForm] FileUploadRequest request)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var candidateCard = await _context.CandidateCards.ByIdOrFailAsync(cardId);

        if (!currentUser.Has(Role.Admin) &&
            !currentUser.IsMyOrganization(candidateCard.OrganizationId))
        {
            return Forbid();
        }

        var uploadedFile = await _storage.UploadFileAsync(new UploadFileServiceRequest(request.File));
        if (!uploadedFile.Result)
        {
            return BadRequest($"The error during upload: {uploadedFile.ErrorReason}");
        }

        candidateCard.AddFile(
            request.File.FileName,
            uploadedFile.UniqueStorageName);

        await _context.TrySaveChangesAsync();
        return Ok(new CandidateCardDto(candidateCard));
    }

    [HttpGet("{cardId:guid}/download/{fileId:guid}")]
    public async Task<IActionResult> DownloadAsync(
        [FromRoute] Guid cardId,
        [FromRoute] Guid fileId)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var candidateCard = await _context.CandidateCards.ByIdOrFailAsync(cardId);

        if (!currentUser.Has(Role.Admin) &&
            !currentUser.IsMyOrganization(candidateCard.OrganizationId))
        {
            return Forbid();
        }

        var fileFromCard = candidateCard.FindFile(fileId);
        if (fileFromCard is null)
        {
            return BadRequest("File not found");
        }

        var file = await _storage.DownloadFileAsync(fileFromCard.StorageFileName);
        return File(file.Content, file.ContentType, fileFromCard.FileName);
    }

    [HttpDelete("{cardId:guid}/delete/{fileId:guid}")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid cardId,
        [FromRoute] Guid fileId)
    {
        var currentUser = await _auth.CurrentUserOrFailAsync();
        var candidateCard = await _context.CandidateCards.ByIdOrFailAsync(cardId);

        if (!currentUser.Has(Role.Admin) &&
            !currentUser.IsMyOrganization(candidateCard.OrganizationId))
        {
            return Forbid();
        }

        var fileFromCard = candidateCard.FindFile(fileId);
        if (fileFromCard is null)
        {
            return BadRequest("File not found");
        }

        await _storage.DeleteFileAsync(fileFromCard.StorageFileName);
        candidateCard.RemoveFile(fileFromCard.StorageFileName);
        await _context.TrySaveChangesAsync();
        return Ok(new CandidateCardDto(candidateCard));
    }
}