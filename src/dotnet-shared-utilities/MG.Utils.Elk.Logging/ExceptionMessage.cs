namespace MG.Utils.Elk.Logging
{
    public static class ExceptionMessage
    {
        public const string LoggerNotCreated = "You have to create the Logger first";
        public const string LoggerNotInitiated = "You have to try to initiate the Logger first";
        public const string ElkIsNotAvailable = "Logging to ELK is not available. Started to log in file";
        public const string FailedToSubmitEvent = "Unable to submit event {0}";
    }
}
