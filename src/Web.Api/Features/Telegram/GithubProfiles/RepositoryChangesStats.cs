namespace Web.Api.Features.Telegram.GithubProfiles;

public record RepositoryChangesStats
{
    public int CommitsCount { get; }

    public int FilesAdjusted { get; }

    public int ChangesInFilesCount { get; }

    public int AdditionsInFilesCount { get; }

    public int DeletionsInFilesCount { get; }

    public RepositoryChangesStats(
        int commitsCount,
        int filesAdjusted,
        int changesInFilesCount,
        int additionsInFilesCount,
        int deletionsInFilesCount)
    {
        CommitsCount = commitsCount;
        FilesAdjusted = filesAdjusted;
        ChangesInFilesCount = changesInFilesCount;
        AdditionsInFilesCount = additionsInFilesCount;
        DeletionsInFilesCount = deletionsInFilesCount;
    }
}