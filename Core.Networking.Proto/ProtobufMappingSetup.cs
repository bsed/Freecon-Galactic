namespace Freecon.Core.Networking.Proto
{
    public static class ProtobufMappingSetup
    {
        private static bool _isSetup = false;

        public static void Setup()
        {
            // Ensure we don't set this up twice.
            if (_isSetup)
            {
                return;
            }

            _isSetup = true;

            //Mapper.Map<PositionUpdateProto, PositionUpdate>();
            //Mapper.Map<PositionUpdate, PositionUpdateProto>();
            //Mapper.Map<MessageContainerProto, RawMessageContainer>();
            //Mapper.Map<RawMessageContainer, MessageContainerProto>();

        }
    }
}
