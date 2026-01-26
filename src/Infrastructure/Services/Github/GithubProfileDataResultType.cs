namespace Infrastructure.Services.Github;

public enum GithubProfileDataResultType
{
    Undefined = 0,

    Success = 1,

    NotFound = 2,

    Failure = 3,

    RateLimitExceeded = 4,
}