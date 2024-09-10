﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Salaries.GetSalariesChart.Charts;
using Web.Api.Features.Salaries.Models;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Salaries.GetSalariesChart
{
    public class GetSalariesChartHandler : IRequestHandler<GetSalariesChartQuery, SalariesChartResponse>
    {
        private readonly IAuthorization _auth;
        private readonly DatabaseContext _context;
        private readonly ICurrencyService _currencyService;

        public GetSalariesChartHandler(
            IAuthorization auth,
            DatabaseContext context,
            ICurrencyService currencyService)
        {
            _auth = auth;
            _context = context;
            _currencyService = currencyService;
        }

        public async Task<SalariesChartResponse> Handle(
            ISalariesChartQueryParams request,
            CancellationToken cancellationToken)
        {
            var currentUser = await _auth.CurrentUserOrNullAsync(cancellationToken);

            var userSalariesForLastYear = new List<UserSalary>();

            var now = DateTimeOffset.Now;
            var salariesQuery = new SalariesForChartQuery(
                _context,
                request,
                now);

            if (currentUser != null)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Id == currentUser.Id, cancellationToken);

                userSalariesForLastYear = await _context.Salaries
                    .Where(x => x.UserId == user.Id)
                    .Where(x => x.Year == salariesQuery.CurrentQuarter.Year || x.Year == salariesQuery.CurrentQuarter.Year - 1)
                    .AsNoTracking()
                    .OrderByDescending(x => x.Year)
                    .ThenByDescending(x => x.Quarter)
                    .ToListAsync(cancellationToken);
            }

            var query = salariesQuery.ToQueryable();
            if (currentUser == null || !userSalariesForLastYear.Any())
            {
                var salaryValues = await query
                    .Select(x => new { x.Company, x.Value })
                    .ToListAsync(cancellationToken);

                var totalCount = await query.CountAsync(cancellationToken);
                return SalariesChartResponse.RequireOwnSalary(
                    salaryValues.Select(x => (x.Company, x.Value)).ToList(),
                    totalCount,
                    true,
                    currentUser is not null);
            }

            var salaries = await query.ToListAsync(cancellationToken);
            var hasSurveyRecentReply = await new SalariesSurveyUserService(_context)
                .HasFilledSurveyAsync(currentUser, cancellationToken);

            var currencies = await GetCurrenciesAsync(
                request,
                cancellationToken);

            return new SalariesChartResponse(
                salaries,
                new UserSalaryAdminDto(userSalariesForLastYear.First()),
                hasSurveyRecentReply,
                salariesQuery.From,
                salariesQuery.To,
                currencies);
        }

        public Task<SalariesChartResponse> Handle(
            GetSalariesChartQuery request,
            CancellationToken cancellationToken)
        {
            return Handle((ISalariesChartQueryParams)request, cancellationToken);
        }

        private async Task<List<CurrencyContent>> GetCurrenciesAsync(
            ISalariesChartQueryParams request,
            CancellationToken cancellationToken)
        {
            if (request.SalarySourceType is SalarySourceType.KolesaDevelopersCsv2022)
            {
                var date = new DateTime(2021, 12, 31);
                return
                [
                    new CurrencyContent(
                        1,
                        Currency.KZT,
                        date),

                    new CurrencyContent(
                        434,
                        Currency.USD,
                        date),

                    new CurrencyContent(
                        491,
                        Currency.EUR,
                        date)

                ];
            }

            return await _currencyService
                .GetAllCurrenciesAsync(cancellationToken);
        }
    }
}