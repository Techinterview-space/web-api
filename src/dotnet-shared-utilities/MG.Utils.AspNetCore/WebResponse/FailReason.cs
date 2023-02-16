namespace MG.Utils.AspNetCore.WebResponse
{
    public enum FailReason
    {
        Undefined = 0,
        ServerIsNotAvailable,
        Timeout,
        NotFound,
        UnexpectedBehavior
    }
}