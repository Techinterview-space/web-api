namespace MG.Utils.Interfaces
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}