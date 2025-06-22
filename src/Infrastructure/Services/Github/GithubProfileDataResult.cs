using Domain.Entities.Github;

namespace Infrastructure.Services.Github;

public record GithubProfileDataResult
{
    public static GithubProfileDataResult NotFound()
    {
        return new GithubProfileDataResult(
            GithubProfileDataResultType.NotFound,
            null,
            "GitHub profile not found.");
    }

    public static GithubProfileDataResult Success(
        GithubProfileData data)
    {
        return new GithubProfileDataResult(
            GithubProfileDataResultType.Success,
            data,
            null);
    }

    public static GithubProfileDataResult Failure(
        string errorMessage)
    {
        return new GithubProfileDataResult(
            GithubProfileDataResultType.Failure,
            null,
            errorMessage);
    }

    private GithubProfileDataResult(
        GithubProfileDataResultType result,
        GithubProfileData data,
        string errorMessage)
    {
        Result = result;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public GithubProfileDataResultType Result { get; }

    public GithubProfileData Data { get; }

    public string ErrorMessage { get; }
}