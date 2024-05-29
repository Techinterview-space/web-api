using System;

namespace Domain.Entities.Interviews
{
    public class ShareLink : BaseModel
    {
        protected ShareLink()
        {
        }

        public ShareLink(
            Interview interview)
        {
            InterviewId = interview.Id;
            ShareToken = Guid.NewGuid();
        }

        public Guid? ShareToken { get; protected set; }

        public Guid? InterviewId { get; protected set; }

        public virtual Interview Interview { get; protected set; }

        public ShareLink RevokeToken()
        {
            ShareToken = Guid.NewGuid();
            UpdatedAt = DateTime.UtcNow;

            return this;
        }
    }
}
