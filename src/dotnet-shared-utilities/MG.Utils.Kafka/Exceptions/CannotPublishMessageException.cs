using System;

namespace MG.Utils.Kafka.Exceptions
{
    public class CannotPublishMessageException<TMessage> : InvalidOperationException
        where TMessage : class, new()
    {
        public CannotPublishMessageException(Exception innerException)
            : base($"Cannot publish {typeof(TMessage).Name} due to internal error", innerException)
        {
        }
    }
}