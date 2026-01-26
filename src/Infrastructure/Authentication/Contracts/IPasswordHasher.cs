namespace Infrastructure.Authentication.Contracts;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}
