﻿using System.Threading.Tasks;
using Domain.Attributes;
using Domain.Authentication.Abstract;
using Domain.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("api/account")]
[HasAnyRole]
public class AccountController : ControllerBase
{
    private readonly IAuthorization _auth;

    public AccountController(IAuthorization auth)
    {
        _auth = auth;
    }

    [HttpGet("me")]
    public async Task<UserDto> MeAsync()
    {
        return new (await _auth.CurrentUserAsync());
    }
}