﻿using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Users.Models;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Users;

[HasAnyRole]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public UsersController(IAuthorization auth, DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpGet("{id:long}")]
    public async Task<UserDto> UserAsync([FromRoute] long id)
    {
        return new UserDto(await _context.Users
            .Include(x => x.UserRoles)
            .AsNoTracking()
            .ByIdOrFailAsync(id));
    }
}