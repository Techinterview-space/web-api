using System.Collections.Generic;
using Domain.Database;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Services.Labels;

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