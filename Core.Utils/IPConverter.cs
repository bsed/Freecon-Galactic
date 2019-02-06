namespace Freecon.Core.Utils
{
    public class IPConverter
    {
        static public string Convert(byte[] IP)
        {
            string result = IP[0].ToString();
            for(int i = 1; i < IP.Length;i++)
            {
                result += ("." + IP[i].ToString());

            }
            return result;
        }

    }
}
