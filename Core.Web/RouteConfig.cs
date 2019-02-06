namespace Core.Web
{
    public static class RouteConfig
    {
        public const string Admin = "/admin";
        public const string Admin_GetAllSystems = "/get-all-systems";
        public const string Admin_WarpPlayer = "/warp-player";
        public const string Admin_WarpPlayerToType = "/warp-player-to-type";
        public const string Admin_GetMockGalaxy = "/mock-galaxy";
        public const string Admin_DirectRoute = "/route__{jsonNetworkMessageContainer}";//Must send NetworkMessageContainer serialized as JSON with the property RoutingData set

        public const string Colony = "/colony";
        public const string Colony_GetLandData = "/land-data";
        public const string Colony_GetColonyData = "/fetch-state";
        public const string Colony_PushState = "/push-state";

        public const string Galaxy = "/galaxy";
        public const string Galaxy_GetSystemInfo = "/system-info/{systemid}";

        public const string Holdings = "/holdings";
        public const string Holdings_FetchState = "/fetch-state";

        public const string Login = "/login";
        public const string Login_Request = "/{username}_{password}";
        public const string Login_Logout = "/logout";

        public const string Port = "/port";
        public const string Port_GetState = "/fetch-state";
        public const string Port_GetPlayersInPort = "/players-in-port";
    }
}