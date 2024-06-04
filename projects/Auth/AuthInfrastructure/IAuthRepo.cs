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
    /// Adds the token to the login
    /// </summary>
    /// <param name="token"></param>
    void AddTokenToLogin(Token token);
    /// <summary>
    /// Gets a user by their token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Login GetUserByToken(string token);
    /// <summary>
    /// Gets user by username
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    Login GetUsersByUsername(string username);
}