namespace Freecon.Server.Configs
{
    /// <summary>
    /// Used to serialize data shared by slave servers
    /// </summary>
    public class DBConfigObject
    {
        public int DBConfigObjectID = 1;//This should always be the same. Hacky way to ensure a single record is stored. Probably need to use something other than EF to store a singleton

        public int SolID;

    }
}
