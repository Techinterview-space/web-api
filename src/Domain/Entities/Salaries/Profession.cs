using System.Collections.Generic;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Entities.Salaries;

public class Profession : EntityLabelBase
{
    protected Profession()
    {
    }

    internal Profession(
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
}