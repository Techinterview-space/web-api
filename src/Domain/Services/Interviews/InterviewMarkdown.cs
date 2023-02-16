using System.Linq;
using System.Text;
using Domain.Entities.Enums;
using Domain.Entities.Interviews;
using Domain.Services.MD;
using MG.Utils.Abstract.Extensions;

namespace Domain.Services.Interviews;

public class InterviewMarkdown
{
    public const string DateFormat = "yyyy-MM-dd";

    private readonly Interview _interview;

    private string _rendered = null;

    public InterviewMarkdown(
        Interview interview)
    {
        _interview = interview;
    }

    public override string ToString()
    {
        if (_rendered == null)
        {
            var builder = new StringBuilder();
            builder.Append(GetHeader());
            builder.Append(GetOverallOpinion());
            builder.Append(GetSubjects());

            _rendered = builder.ToString();
        }

        return _rendered;
    }

    private string GetHeader()
    {
        var builder = new StringBuilder();
        builder.AppendLine(MarkdownItems.H1($"Interview with {_interview.CandidateName}"));
        builder.AppendLine();
        builder.Append(MarkdownItems.List(
                $"Grade: {MarkdownItems.Bold(GradeAsString(_interview.CandidateGrade))}",
                $"Interviewer: {_interview.Interviewer.Fullname}, {_interview.Interviewer.Email}",
                $"When: {_interview.CreatedAt.ToString(DateFormat)}"));

        return builder.ToString();
    }

    private string GetOverallOpinion()
    {
        var builder = new StringBuilder();
        builder.AppendLine(MarkdownItems.H2("Overall Opinion"));
        builder.AppendLine();
        builder.AppendLine(_interview.OverallOpinion);
        builder.AppendLine();

        return builder.ToString();
    }

    private string GetSubjects()
    {
        var builder = new StringBuilder();
        builder.AppendLine(MarkdownItems.H2("Subjects"));
        builder.AppendLine();

        if (!_interview.Subjects.Any())
        {
            return builder.ToString();
        }

        foreach (var subject in _interview.Subjects)
        {
            builder.AppendLine(MarkdownItems.H4($"{subject.Title} - {GradeAsString(subject.Grade)}"));
            builder.AppendLine();

            if (string.IsNullOrEmpty(subject.Comments))
            {
                continue;
            }

            builder.AppendLine(subject.Comments);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string GradeAsString(
        DeveloperGrade? grade)
    {
        return !grade.HasValue ? "Did not check" : grade.Value.Description();
    }
}