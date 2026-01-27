namespace Web.Api.Features.Emails.ViewModels;

public abstract record ViewModelBase
{
    protected ViewModelBase(
        string unsubscribeToken)
    {
        UnsubscribeToken = unsubscribeToken?.Trim();
    }

    public string UnsubscribeToken { get; }

    public bool HaveUnsubscribeToken
        => !string.IsNullOrEmpty(UnsubscribeToken);
}