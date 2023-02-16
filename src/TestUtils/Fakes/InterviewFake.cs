using System.Collections.Generic;
using Domain.Entities.Enums;
using Domain.Entities.Interviews;
using Domain.Entities.Users;
using TestUtils.Db;

namespace TestUtils.Fakes;

public class InterviewFake : Interview, IPlease<Interview>
{
    public InterviewFake(
        User interviewer)
    {
        InterviewerId = interviewer.Id;
        Interviewer = interviewer;
        CandidateName = "Jorn Smith";
        CandidateGrade = DeveloperGrade.Middle;
        OverallOpinion = "Open mind person";
        Subjects = new List<InterviewSubject>
        {
            new ()
            {
                Title = "ASP.NET Core",
                Grade = DeveloperGrade.Middle,
                Comments = "Middlewares, Caching"
            },
            new ()
            {
                Title = "SQL",
                Grade = null,
            }
        };
    }

    public Interview Please() => this;

    public IPlease<Interview> AsPlease() => this;
}