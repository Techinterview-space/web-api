using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Companies;

public class CompanyOpenAiAnalysis : HasDatesBase, IHasIdBase<Guid>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; protected set; }

    public string AnalysisText { get; protected set; }

    public string Model { get; protected set; }

    public Guid CompanyId { get; protected set; }

    public virtual Company Company { get; protected set; }

    public CompanyOpenAiAnalysis(
        Company company,
        string analysisText,
        string model)
    {
        Id = Guid.NewGuid();
        CompanyId = company.Id;
        Model = model;
        AnalysisText = analysisText ?? throw new ArgumentNullException(nameof(analysisText));
    }

    protected CompanyOpenAiAnalysis()
    {
    }
}