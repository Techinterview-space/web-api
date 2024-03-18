using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Salaries;
using Domain.Tools;

namespace Domain.Salaries;

public class DeveloperProfessionsCollection : IEnumerable<long>
{
    private readonly List<long> _professionsToInclude;
    private readonly bool _extendDeveloperProfession;

    public DeveloperProfessionsCollection(
        params UserProfessionEnum[] professionsToInclude)
        : this(
            professionsToInclude.Select(x => (long)x).ToList(),
            false)
    {
    }

    public DeveloperProfessionsCollection(
        List<long> professionsToInclude,
        bool extendDeveloperProfession = true)
    {
        _professionsToInclude = professionsToInclude;
        _extendDeveloperProfession = extendDeveloperProfession;
    }

    public IEnumerator<long> GetEnumerator()
    {
        if (_professionsToInclude.Count > 0 &&
            _professionsToInclude.Contains((long)UserProfessionEnum.Developer) &&
            _extendDeveloperProfession)
        {
            _professionsToInclude.AddIfDoesNotExist(
                (long)UserProfessionEnum.BackendDeveloper,
                (long)UserProfessionEnum.FrontendDeveloper,
                (long)UserProfessionEnum.FullstackDeveloper,
                (long)UserProfessionEnum.MobileDeveloper,
                (long)UserProfessionEnum.IosDeveloper,
                (long)UserProfessionEnum.AndroidDeveloper,
                (long)UserProfessionEnum.GameDeveloper);
        }

        return _professionsToInclude.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}