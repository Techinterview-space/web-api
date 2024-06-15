using System.Collections.Generic;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;
using Infrastructure.Database;

namespace Web.Api.Features.Labels.Models;

public class UserLabelsCollection : LabelsCollectionBase<UserLabel>
{
    public UserLabelsCollection(
        DatabaseContext context, User currentUser, IReadOnlyCollection<LabelDto> labelsFromRequest)
        : base(context, currentUser, labelsFromRequest)
    {
    }

    protected override UserLabel CreateLabelFromDto(LabelDto label) =>
        new (
            label.Title,
            new HexColor(label.HexColor),
            CurrentUser);
}