using Domain.PostgressEntities;

public interface IAuthRepo
{
    /// <summary>
    /// Adds a new login
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    Login AddLogin(Login login);


    /// <summary>
    /// Gets user by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Login GetUsersByUsername(string username);
}