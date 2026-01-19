using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Domain.Entities.Users;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Companies;

public class CompanyReview : HasDatesBase, IHasIdBase<Guid>
{
    public const int YearsBeforeOutdated = 2;

    public Guid Id { get; protected set; }

    public int CultureAndValues { get; protected set; }

    public int? CodeQuality { get; protected set; }

    public int WorkLifeBalance { get; protected set; }

    public int Management { get; protected set; }

    public int CompensationAndBenefits { get; protected set; }

    public int CareerOpportunities { get; protected set; }

    public double TotalRating { get; protected set; }

    public string Pros { get; protected set; }

    public string Cons { get; protected set; }

    public bool IWorkHere { get; protected set; }

    public int LikesCount { get; protected set; }

    public int DislikesCount { get; protected set; }

    public CompanyEmploymentType UserEmployment { get; protected set; }

    public DateTime? ApprovedAt { get; protected set; }

    public DateTime? OutdatedAt { get; protected set; }

    public Guid CompanyId { get; protected set; }

    public virtual Company Company { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public virtual List<CompanyReviewVote> Votes { get; protected set; }

    public CompanyReview(
        int cultureAndValues,
        int? codeQuality,
        int workLifeBalance,
        int management,
        int compensationAndBenefits,
        int careerOpportunities,
        string pros,
        string cons,
        bool workHere,
        CompanyEmploymentType userEmployment,
        Company company,
        User user)
    {
        CultureAndValues = ValidateRating(cultureAndValues);
        CodeQuality = ValidateRating(codeQuality);
        WorkLifeBalance = ValidateRating(workLifeBalance);
        Management = ValidateRating(management);
        CompensationAndBenefits = ValidateRating(compensationAndBenefits);
        CareerOpportunities = ValidateRating(careerOpportunities);

        if (CodeQuality.HasValue)
        {
            TotalRating = CalculateTotal(
                CultureAndValues,
                CodeQuality.Value,
                WorkLifeBalance,
                Management,
                CompensationAndBenefits,
                CareerOpportunities);
        }
        else
        {
            TotalRating = CalculateTotal(
                CultureAndValues,
                WorkLifeBalance,
                Management,
                CompensationAndBenefits,
                CareerOpportunities);
        }

        Pros = pros;
        Cons = cons;
        IWorkHere = workHere;
        UserEmployment = userEmployment;
        ApprovedAt = null;
        OutdatedAt = null;
        CompanyId = company.Id;
        UserId = user.Id;
    }

    protected CompanyReview()
    {
    }

    public double? GetLikesRateOrNull()
    {
        if (LikesCount == 0 && DislikesCount == 0)
        {
            return null;
        }

        return Math.Round(
            (double)LikesCount / (LikesCount + DislikesCount) * 100);
    }

    public bool IsRelevant()
    {
        return ApprovedAt != null &&
               OutdatedAt == null;
    }

    public void Approve(
        bool calledByCompany = false)
    {
        if (ApprovedAt != null)
        {
            return;
        }

        if (Company is null)
        {
            throw new BadRequestException("Company is null");
        }

        ApprovedAt = DateTime.UtcNow;
        OutdatedAt = null;

        if (!calledByCompany)
        {
            Company.ApproveReview(Id);
        }
    }

    public void MarkAsOutdated()
    {
        if (OutdatedAt != null)
        {
            throw new BadRequestException("Review is already marked as outdated");
        }

        OutdatedAt = DateTime.UtcNow;
    }

    public bool AddVote(
        User reviewer,
        CompanyReviewVoteType voteType)
    {
        if (Votes.Any(x => x.UserId == reviewer.Id))
        {
            return false;
        }

        Votes.Add(new CompanyReviewVote(
            this,
            reviewer.Id,
            voteType));

        if (voteType == CompanyReviewVoteType.Like)
        {
            LikesCount++;
        }
        else if (voteType == CompanyReviewVoteType.Dislike)
        {
            DislikesCount++;
        }
        else
        {
            throw new InvalidOperationException("Vote type must be like or dislike");
        }

        return true;
    }

    protected static double CalculateTotal(
        params int[] ratings)
    {
        return ratings.Length == 0
            ? 0
            : ratings.Average();
    }

    private static int? ValidateRating(int? rating)
    {
        if (rating == null)
        {
            return null;
        }

        return ValidateRating(rating.Value);
    }

    private static int ValidateRating(int rating)
    {
        if (rating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5");
        }

        return rating;
    }
}