using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Users;

public class UserEmail : HasDatesBase
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; protected set; }

    public UserEmailType Type { get; protected set; }

    public string EmailSubject { get; protected set; }

    public long UserId { get; protected set; }

    public virtual User User { get; protected set; }

    public UserEmail(
        string subject,
        UserEmailType type,
        User user)
    {
        subject = subject?.Trim();
        EmailSubject = subject ?? throw new ArgumentNullException(nameof(subject));
        Type = type;
        UserId = user.Id;
    }

    protected UserEmail()
    {
    }
}