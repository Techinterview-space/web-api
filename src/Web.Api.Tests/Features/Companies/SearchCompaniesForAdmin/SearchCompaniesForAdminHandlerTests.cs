using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Companies.SearchCompaniesForAdmin;
using Xunit;

namespace Web.Api.Tests.Features.Companies.SearchCompaniesForAdmin;

public class SearchCompaniesForAdminHandlerTests
{
    [Fact]
    public async Task Handle_WithCompanyNameFilter_ReturnsMatchingCompanies()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Google LLC");
        var company3 = CreateAndSaveCompany(context, "Microsoft Azure");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10,
            CompanyName = "Microsoft"
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        Assert.Equal(2, result.Results.Count);
        Assert.Equal(2, result.TotalItems);
        Assert.Contains(result.Results, x => x.Id == company1.Id);
        Assert.Contains(result.Results, x => x.Id == company3.Id);
        Assert.DoesNotContain(result.Results, x => x.Id == company2.Id);
    }

    [Fact]
    public async Task Handle_WithSearchQuery_ReturnsMatchingCompanies()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Google LLC");
        var company3 = CreateAndSaveCompany(context, "Apple Inc");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10,
            SearchQuery = "Google"
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        Assert.Single(result.Results);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(company2.Id, result.Results.First().Id);
    }

    [Fact]
    public async Task Handle_WithBothFilters_ReturnsCompaniesMatchingBoth()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Microsoft Azure");
        var company3 = CreateAndSaveCompany(context, "Google Microsoft");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10,
            SearchQuery = "Corporation",
            CompanyName = "Microsoft"
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        // Should match companies that contain both "Corporation" (SearchQuery) and "Microsoft" (CompanyName)
        Assert.Single(result.Results);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(company1.Id, result.Results.First().Id);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllCompanies()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Google LLC");
        var company3 = CreateAndSaveCompany(context, "Apple Inc");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        Assert.Equal(3, result.Results.Count);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task Handle_WithEmptyCompanyNameFilter_IgnoresFilter()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Google LLC");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10,
            CompanyName = ""
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        Assert.Equal(2, result.Results.Count);
        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_CompanyNameFilter_IsCaseInsensitive()
    {
        await using var context = new InMemoryDatabaseContext();
        
        var company1 = CreateAndSaveCompany(context, "Microsoft Corporation");
        var company2 = CreateAndSaveCompany(context, "Google LLC");

        var target = new SearchCompaniesForAdminHandler(context);

        var queryParams = new SearchCompaniesForAdminQueryParams
        {
            Page = 1,
            PageSize = 10,
            CompanyName = "MICROSOFT"
        };

        var result = await target.Handle(queryParams, CancellationToken.None);

        Assert.Single(result.Results);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(company1.Id, result.Results.First().Id);
    }

    private static Company CreateAndSaveCompany(InMemoryDatabaseContext context, string name)
    {
        var company = new Company(name, "Test description", new List<string>(), "");
        var entry = context.Companies.Add(company);
        context.SaveChanges();
        return entry.Entity;
    }
}