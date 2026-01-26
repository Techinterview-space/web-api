using System;
using System.Threading.Tasks;
using Domain.Entities.CSV;
using Domain.Entities.Users;
using Infrastructure.Database;

namespace Web.Api.Tests.Features.Salaries.ExportCsv;

public class UserCsvDownloadFake : UserCsvDownload
{
    public UserCsvDownloadFake(
        User user,
        DateTime? createdAt = null)
        : base(user)
    {
        if (createdAt.HasValue)
        {
            CreatedAt = createdAt.Value;
        }
    }

    public async Task<UserCsvDownload> PleaseAsync(
        DatabaseContext context)
    {
        var entity = await context.SaveAsync((UserCsvDownload)this);
        return entity;
    }
}