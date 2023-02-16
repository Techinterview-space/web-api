using System.Collections.Generic;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using MG.Utils.ValueObjects;

namespace Domain.Entities.Labels;

public class UserLabel : EntityLabelBase
{
    protected UserLabel()
    {
    }

    public UserLabel(
        string title,
        HexColor hexcolor = null,
        User createdBy = null)
        : base(title, hexcolor, createdBy)
    {
    }

    public virtual ICollection<InterviewTemplate> InterviewTemplates { get; protected set; } = new List<InterviewTemplate>();

    public virtual ICollection<Interview> Interviews { get; protected set; } = new List<Interview>();
}