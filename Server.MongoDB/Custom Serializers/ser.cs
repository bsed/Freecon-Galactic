using MongoDB.Bson.Serialization.Serializers;

namespace Server.MongoDB.Custom_Serializers
{
    public class MyEnumSerializer : StructSerializerBase<int>
    {
        //public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, int value)
        //{
        //    BsonSerializer.Serialize(context.Writer, value.ToString());
        //}
        //public override int Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        //{
        //    return (int)Enum.Parse(args.NominalType, BsonSerializer.Deserialize<string>(context.Reader));
        //}
    }
}
