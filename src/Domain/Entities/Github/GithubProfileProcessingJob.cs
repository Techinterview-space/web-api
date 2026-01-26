namespace Domain.Entities.Github;

public class GithubProfileProcessingJob : HasDatesBase
{
    public string Username { get; protected set; }

    public GithubProfileProcessingJob(
        string username)
    {
        Username = username?.Trim();
    }

    protected GithubProfileProcessingJob()
    {
    }
}