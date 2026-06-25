using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Companies.GetAllCompanies;
using Xunit;

namespace Web.Api.Tests.Features.Companies.GetAllCompanies;

public class GetAllCompaniesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsActiveCompanies_WithLeanShape_NotPaginated()
    {
        await using var context = new InMemoryDatabaseContext();

        var company1 = new CompanyFake().Please(context);
        var company2 = new CompanyFake().Please(context);

        var deleted = new CompanyFake();
        deleted.Delete();
        context.Companies.Add(deleted);
        await context.SaveChangesAsync();

        var target = new GetAllCompaniesHandler(context);

        var result = await target.Handle(
            new GetAllCompaniesQuery(),
            CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => x.Id == deleted.Id);

        var item = result.First(x => x.Id == company1.Id);
        Assert.Equal(company1.Name, item.Name);
        Assert.Equal(company1.Description, item.Description);
        Assert.Equal(company1.Slug, item.Slug);
        Assert.Equal(company1.Links, item.Links);
        Assert.Null(item.AiAnalysis);

        Assert.Contains(result, x => x.Id == company2.Id);
    }
}
