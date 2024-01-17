using Domain.Services.Salaries;

namespace TechInterviewer.Controllers.Salaries.CreateSalaryRecord;

public record CreateSalaryRecordResponse
{
    public bool IsSuccess { get; private init; }

    public string ErrorMessage { get; private init; }

    public UserSalaryDto CreatedSalary { get; private init; }

    public static CreateSalaryRecordResponse Success(
        UserSalaryDto createdSalary)
    {
        return new ()
        {
            IsSuccess = true,
            CreatedSalary = createdSalary,
            ErrorMessage = null,
        };
    }

    public static CreateSalaryRecordResponse Failure(
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