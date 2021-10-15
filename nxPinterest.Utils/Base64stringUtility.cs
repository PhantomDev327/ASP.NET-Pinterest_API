using System;
using System.Text;

namespace nxPinterest.Utils
{
    public class Base64stringUtility
    {
        private Encoding enc;

        public Base64stringUtility(string encodeString)
        {
            enc = Encoding.GetEncoding(encodeString);
        }

        public string Encode(string str)
        {
            return Convert.ToBase64String(enc.GetBytes(str));
        }

        public string Decode(string str)
        {
            return enc.GetString(Convert.FromBase64String(str));
        }
    }
}
