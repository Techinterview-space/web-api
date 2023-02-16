using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MG.Utils.Abstract;
using MG.Utils.Abstract.Extensions;

namespace MG.Utils.Smtp
{
    public class EmailMessage
    {
        // For deserialization purposes only
        public EmailMessage()
        {
        }

        public EmailMessage(
            string subject,
            IReadOnlyCollection<string> to,
            string body)
        {
            subject.ThrowIfNullOrEmpty(nameof(subject));
            to.ThrowIfNullOrEmpty(nameof(to));
            body.ThrowIfNullOrEmpty(nameof(body));

            To = to.ToList();
            Subject = subject;
            Body = body;

            Cc = new List<string>();
            Bcc = new List<string>();
        }

        public EmailMessage(
            string subject,
            string to,
            string body)
            : this(subject, new[] { to }, body)
        {
            to.ThrowIfNullOrEmpty(nameof(to));
        }

        public EmailMessage AddCc(params string[] cc)
        {
            if (cc.Any())
            {
                Cc.AddRange(cc);
            }

            return this;
        }

        public EmailMessage AddBcc(params string[] bcc)
        {
            if (bcc.Any())
            {
                Bcc.AddRange(bcc);
            }

            return this;
        }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public ICollection<string> To { get; set; }

        public ICollection<string> Cc { get; set; }

        public ICollection<string> Bcc { get; set; }

        public string DebugInfo()
        {
            var to = To.Count == 1
                ? To.First()
                : To.Aggregate(string.Empty, (curr, next) => $"{curr}, {next}");

            return $"To: {to}\r\n" +
                   $"Subject: {Subject}\r\n" +
                   $"Body: {Body.Length}";
        }
    }
}