using System;

namespace Domain.Entities.Interviews
{
    public class ShareLink : IHasId
    {
        protected ShareLink()
        {
        }

        public ShareLink(Interview interview)
        {
            InterviewId = interview.Id;
            ShareToken = Guid.NewGuid();
        }

        public long Id { get; protected set; }

        public Guid? ShareToken { get; protected set; }

        public Guid InterviewId { get; protected set; }

        public Interview Interview { get; protected set; }

        public ShareLink RevokeToken()
        {
            ShareToken = Guid.NewGuid();
            return this;
        }
    }
}
