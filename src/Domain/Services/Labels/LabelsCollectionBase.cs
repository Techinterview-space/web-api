﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using MG.Utils.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services.Labels;

public abstract class LabelsCollectionBase<TLabel>
where TLabel : EntityLabelBase
{
    protected User CurrentUser { get; }

    private readonly DatabaseContext _context;
    private readonly IReadOnlyCollection<LabelDto> _labelsFromRequest;

    protected LabelsCollectionBase(
        DatabaseContext context,
        User currentUser,
        IReadOnlyCollection<LabelDto> labelsFromRequest)
    {
        _context = context;
        _labelsFromRequest = labelsFromRequest;
        CurrentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<TLabel>> PrepareAsync()
    {
        var labelIdsToStay = _labelsFromRequest
            .Where(x => x.Id != null)
            .Select(x => x.Id)
            .ToList();

        var listToSync = new List<TLabel>();
        listToSync.AddRange(await _context.Set<TLabel>().Where(x => labelIdsToStay.Contains(x.Id)).ToArrayAsync());

        var labelsToAdd = _labelsFromRequest
            .Where(x => x.Id == null)
            .Select(CreateLabelFromDto);

        foreach (var userLabel in labelsToAdd)
        {
            if (await _context.Set<TLabel>().AnyAsync(x => x.Title == userLabel.Title))
            {
                continue;
            }

            listToSync.Add(userLabel);
        }

        return listToSync;
    }

    protected abstract TLabel CreateLabelFromDto(LabelDto label);
}