using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoomBookingAPI.Model
{
    public class Authentication
    {
        private MyDbContext context;

        public bool TryAuthenticate(string name, string password, out Account acc)
        {
            acc = context.Accounts.AsQueryable().FirstOrDefault(a => a.Password == password && a.Name == name);
            if (acc != null)
                return true;
            return false;
        }

        public bool TryCreateToken(string name, string password, out string token)
        {
            token = null;
            if (TryAuthenticate(name, password, out var acc))
            {
                token = CreateAuthToken(acc);
                return true;
            }

            return false;
        }

        public bool TryCheckToken(string token)
        {
            try
            {
                var tokenSplit = token.Split('.');
                var headerJson64 = tokenSplit[0];
                var payloadJson64 = tokenSplit[1];
                var signature = tokenSplit[2];

                var payloadJson = Convert.FromBase64String(payloadJson64.Replace('-', '+').Replace('_', '/').PadRight(4 * ((payloadJson64.Length + 3) / 4), '='));
                var payload = JsonSerializer.Deserialize<JWTPayload>(payloadJson);

                var acc = context.Accounts.AsQueryable().FirstOrDefault(a => a.Name == payload.user);

                if (signature == CreateSignature(headerJson64, payloadJson64, acc))
                    return true;
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        private string CreateAuthToken(Account acc)
        {
            var header = new JWTHeader() { alg = "SHA256", typ = "JWT" };
            var payload = new JWTPayload() { user = acc.Name };
            
            var headerJson64 = Base64UrlEncoder.Encode(JsonSerializer.Serialize(header));
            var payloadJson64 = Base64UrlEncoder.Encode(JsonSerializer.Serialize(payload));
            var signature = CreateSignature(headerJson64, payloadJson64, acc);


            return $"{headerJson64}.{payloadJson64}.{signature}";
        }

        private string CreateSignature(string header, string payload, Account acc)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var stringToEncode = header + "." + payload + "-" + acc.Id + acc.Password;
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(stringToEncode));

                var hashStringBuilder = new StringBuilder();

                foreach (Byte b in hash)
                    hashStringBuilder.Append(b.ToString("x2"));

                return hashStringBuilder.ToString();
            }
        }

        public Authentication()
        {
            context = MyDbContext.GetDbContext();
        }
    }

    public class JWTHeader
    {
        public string typ { get; set; }
        public string alg { get; set; }
    }

    public class JWTPayload {
        public string user { get; set;} 
    }
}
