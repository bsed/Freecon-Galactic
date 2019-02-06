namespace Server.Utilities
{
    public static class StringConverter
    {
        public static string GetString(this byte[] t)
        {
            string val = t[0].ToString();
            for (int i = 1; i < t.Length; i++)
            {
                val += ".";
                val += t[i];
            }

            return val;
        }

    }
}
