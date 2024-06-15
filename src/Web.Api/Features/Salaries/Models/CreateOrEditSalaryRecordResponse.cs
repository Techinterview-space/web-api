using Infrastructure.Salaries;

namespace Web.Api.Features.Salaries.Models;

public record CreateOrEditSalaryRecordResponse
{
    public bool IsSuccess { get; private init; }

    public string ErrorMessage { get; private init; }

    public UserSalaryDto CreatedSalary { get; private init; }

    public static CreateOrEditSalaryRecordResponse Success(
        UserSalaryDto createdSalary)
    {
        return new ()
        {
            IsSuccess = true,
            CreatedSalary = createdSalary,
            ErrorMessage = null,
        };
    }

    public static CreateOrEditSalaryRecordResponse Failure(
        string errorMessage)
    {
        return new ()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            CreatedSalary = null,
        };
    }
}