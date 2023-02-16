namespace Domain.Consumers.Contract.Messages
{
    public enum ChangeType
    {
        Undefined = 0,

        Create,

        Update,

        SoftDelete,

        Remove,

        Restore
    }
}