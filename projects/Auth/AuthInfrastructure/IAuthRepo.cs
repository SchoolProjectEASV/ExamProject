using Domain.PostgressEntities;

public interface IAuthRepo
{
    Login AddLogin(Login login);
    void AddTokenToLogin(Token token);
    Login GetUserByToken(string token);
    Login GetUsersByUsername(string username);
}