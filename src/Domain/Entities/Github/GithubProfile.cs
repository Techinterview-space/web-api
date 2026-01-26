using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Github;

public class GithubProfile : HasDatesBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Username { get; protected set; }

    public GithubProfileData Data { get; protected set; }

    public int Version { get; protected set; }

    public int RequestsCount { get; protected set; }

    public DateTime DataSyncedAt { get; protected set; }

    public GithubProfile(
        string username,
        GithubProfileData data)
    {
        Username = username;
        Data = data;
        Version = 1;
        RequestsCount = 0;
        DataSyncedAt = DateTime.UtcNow;
    }

    public GithubProfileData GetProfileDataIfRelevant()
    {
        var relevantDateEdge = DateTime.UtcNow.AddDays(-1);
        return DataSyncedAt > relevantDateEdge ? Data : null;
    }

    public void SyncData(
        GithubProfileData data)
    {
        Data = data;
        DataSyncedAt = DateTime.UtcNow;
        Version++;
    }

    public void IncrementRequestsCount()
    {
        RequestsCount++;
    }

    protected GithubProfile()
    {
    }
}