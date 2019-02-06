using Core.Models.Enums;

namespace Freecon.Core.Networking.Models.ServerToServer
{
    public class MessageIDRequest:MessageServerToServer
    {
        public IDTypes IDType { get; set; }

        public int NumIDsRequested { get; set; }

        public int RequestingServerID { get { return _requestingServerID; } set { _isRequestingServerIDSet = true; _requestingServerID = value; } }
        int _requestingServerID;
        bool _isRequestingServerIDSet;



        public override byte[] Serialize()
        {
            if (!_isRequestingServerIDSet)
            {
                throw new RequiredParameterNotInitialized("TargetServerID", this);
            }


            return base.Serialize();
        }
    }
}
