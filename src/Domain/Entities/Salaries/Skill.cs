﻿using System.Collections.Generic;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Entities.Salaries;

public class Skill : EntityLabelBase
{
    protected Skill()
    {
    }

    public Skill(
        string title,
        HexColor hexcolor = null,
        User createdBy = null)
        : base(title, hexcolor, createdBy)
    {
    }

    public virtual List<UserSalary> Salaries { get; protected set; } = new ();
}