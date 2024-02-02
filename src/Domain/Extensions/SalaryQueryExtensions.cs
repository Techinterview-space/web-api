using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Domain.Extensions;

public static class SalaryQueryExtensions
{
    public static IQueryable<UserSalary> FilterByCitiesIfNecessary(
        this IQueryable<UserSalary> salaries, List<KazakhstanCity> cities)
    {
        if (cities.Count != 0)
        {
            if (cities.Count == 1 && cities[0] == KazakhstanCity.Undefined)
            {
                return salaries.Where(s => s.City == null);
            }

            Expression<Func<UserSalary, bool>> clause = s => s.City != null && cities.Contains(s.City.Value);
            if (cities.Any(x => x == KazakhstanCity.Undefined))
            {
                clause = clause.Or(x => x.City == null);
            }

            return salaries.Where(clause);
        }

        return salaries;
    }
}