using Domain.Entities.Salaries;
using Domain.ValueObjects;

namespace TestUtils.Fakes;

public class ProfessionFake : Profession
{
    public ProfessionFake(
        long id,
        string title)
        : base(
            title, HexColor.Random(), null)
    {
        Id = id;
    }
}