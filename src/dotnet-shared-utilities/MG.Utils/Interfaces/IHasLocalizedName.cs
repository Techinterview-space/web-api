namespace MG.Utils.Interfaces
{
    public interface IHasLocalizedName
    {
        string NameEn { get; }

        string NameRu { get; }

        string Name { get; set; }
    }
}