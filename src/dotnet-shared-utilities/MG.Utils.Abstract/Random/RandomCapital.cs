namespace MG.Utils.Abstract.Random
{
    public record RandomCapital : RandomStringBase
    {
        public RandomCapital(int length)
            : base(length)
        {
        }

        protected override string Chars => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    }
}