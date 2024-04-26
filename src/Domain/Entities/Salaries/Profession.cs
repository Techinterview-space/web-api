using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Entities.Salaries;

public class Profession : EntityLabelBase
{
    protected Profession()
    {
    }

    // only for EF
    public Profession(
        long id,
        string title,
        HexColor hexcolor = null,
        User createdBy = null)
        : this(title, hexcolor, createdBy)
    {
        Id = id;
    }

    public Profession(
        string title,
        HexColor hexcolor = null,
        User createdBy = null)
        : base(title, hexcolor, createdBy)
    {
    }

    public virtual List<UserSalary> Salaries { get; protected set; } = new ();

    public UserProfessionEnum IdAsEnum => (UserProfessionEnum)Id;

    public List<string> SplitTitle() =>
        Title
            .Split(' ')
            .Select(x => x.ToLowerInvariant())
            .ToList();
}