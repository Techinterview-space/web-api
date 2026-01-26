namespace Web.Api.Features.Emails.ViewModels;

public abstract record ViewModelBase
{
    protected ViewModelBase(
        string unsubscribeToken)
    {
        UnsubscribeToken = unsubscribeToken;
    }

    public string UnsubscribeToken { get; }
}