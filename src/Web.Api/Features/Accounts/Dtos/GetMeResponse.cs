using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Users;
using Infrastructure.Salaries;
using Web.Api.Features.Users.Models;

namespace Web.Api.Features.Accounts.Dtos;

public record GetMeResponse : UserDto
{
    public List<UserSalaryDto> Salaries { get; init; }

    public GetMeResponse(
        User user)
        : base(user)
    {
        Salaries = user.Salaries?
            .Select(x => new UserSalaryDto(x))
            .ToList()
                   ?? new List<UserSalaryDto>(0);
    }
}