using Domain.PostgressEntities;

public interface IAuthRepo
{
    Login AddLogin(Login login);
    Login GetUsersByUsername(string username);
}