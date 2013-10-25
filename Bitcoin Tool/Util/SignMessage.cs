using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bitcoin_Tool.Crypto;
using Bitcoin_Tool.DataConverters;
using Bitcoin_Tool.Structs;
using System.Security.Cryptography;

namespace Bitcoin_Tool.Util
{
    public class SignMessage
    {
        private SHA256Managed sha256 = new SHA256Managed();

        public string signMessage(PublicKey pubKey, String message)
        {
            
            return "";
        }
    }
}
