using System.Threading.Tasks;

namespace Core.Database
{
    public class DatabaseHelpers
    {
        public Task<ILoginResult> PerformLoginCheck(ILoginCredentials loginCredentials)
        {
            return Task.FromResult<ILoginResult>(new LoginResult(true, null));
        }
    }

    public interface ILoginResult
    {
        bool Success { get; }

        string Error { get; }
    }

    public class LoginResult : ILoginResult
    {
        public bool Success { get; private set; }

        public string Error { get; private set; }

        public LoginResult(bool success, string error) {
            Success = success;
            Error = error;
        }
    }

    public interface ILoginCredentials 
    {
        string Username { get; }

        string Password { get; }
    }

    public class LoginCredentials : ILoginCredentials
    {
        public string Username { get; private set; }

        public string Password { get; private set; }

        public LoginCredentials(string username, string password) 
        {
            Username = username;
            Password = password;
        }
    }






}
