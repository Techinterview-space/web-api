namespace Web.Api.Features.Github.DeleteGithubProfile;

#pragma warning disable SA1313
public record DeleteGithubProfileCommand(
    string Username);