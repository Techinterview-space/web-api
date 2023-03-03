using System.Collections.Generic;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Entities.Labels;
using Domain.Entities.Users;
using Domain.ValueObjects;

namespace Domain.Services.Labels;

public class CandidateCardLabelsCollection : LabelsCollectionBase<OrganizationLabel>
{
    private readonly CandidateCard _card;

    public CandidateCardLabelsCollection(
        DatabaseContext context,
        User currentUser,
        IReadOnlyCollection<LabelDto> labelsFromRequest,
        CandidateCard card)
        : base(context, currentUser, labelsFromRequest)
    {
        _card = card;
    }

    protected override OrganizationLabel CreateLabelFromDto(LabelDto label) =>
        new (
            label.Title,
            _card.OrganizationId,
            new HexColor(label.HexColor),
            CurrentUser);
}