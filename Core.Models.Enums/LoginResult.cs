namespace Core.Models.Enums
{
    public enum LoginResult
    {
        Success,


        MaxTimeoutsExceeded,//Clientside
        ServerNotReady,//Clientside
        
        AlreadyLoggedOn,
        AlreadyPending,
        InvalidUsernameOrPassword,

        UnknownFailure
    }
}
