using System;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Entities.StatData.Salary;

namespace Domain.Entities.StatData;

public class AiAnalysisRecord : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string AiReportSource { get; protected set; }

    public string AiReport { get; protected set; }

    public double ProcessingTimeMs { get; protected set; }

    public string Model { get; protected set; }

    // TODO rename to SalarySubscriptionId
    public Guid? SubscriptionId { get; protected set; }

    // TODO rename to SalarySubscription
    public virtual StatDataChangeSubscription Subscription { get; protected set; }

    public Guid? CompanyReviewsSubscriptionId { get; protected set; }

    public virtual LastWeekCompanyReviewsSubscription CompanyReviewsSubscription { get; protected set; }

    public AiAnalysisRecord(
        StatDataChangeSubscription salarySubscription,
        string aiReportSource,
        string aiReport,
        double processingTimeMs,
        string model)
        : this(aiReportSource, aiReport, processingTimeMs, model)
    {
        SubscriptionId = salarySubscription.Id;
        Subscription = salarySubscription;
    }

    public AiAnalysisRecord(
        LastWeekCompanyReviewsSubscription companyReviewsSubscription,
        string aiReportSource,
        string aiReport,
        double processingTimeMs,
        string model)
        : this(aiReportSource, aiReport, processingTimeMs, model)
    {
        CompanyReviewsSubscriptionId = companyReviewsSubscription.Id;
    }

    private AiAnalysisRecord(
        string aiReportSource,
        string aiReport,
        double processingTimeMs,
        string model)
    {
        Id = Guid.NewGuid();
        aiReportSource = aiReportSource?.Trim();
        aiReport = aiReport?.Trim();

        if (string.IsNullOrEmpty(aiReportSource))
        {
            throw new ArgumentNullException(nameof(aiReportSource));
        }

        if (string.IsNullOrEmpty(aiReport))
        {
            throw new ArgumentNullException(nameof(aiReport));
        }

        AiReportSource = aiReportSource;
        AiReport = aiReport;
        ProcessingTimeMs = processingTimeMs;
        Model = model?.Trim();
    }

    public string GetClearedReport()
    {
        return AiReport
            .Trim()
            .Trim('`')
            .Trim('\r', '\n');
    }

    protected AiAnalysisRecord()
    {
    }
}