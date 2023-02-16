namespace MG.Utils.Abstract.Entities;

public interface IHasIdBase<out TKey>
    where TKey : struct
{
    TKey Id { get; }
}