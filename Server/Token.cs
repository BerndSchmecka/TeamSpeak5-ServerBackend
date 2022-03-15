using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Server {
    public static class Token {

        public static string GenerateDownloadToken(string uuidHex, string serverId, string homeServer, string roomId) {
            var key = Convert.FromBase64String(Program.downloadTokenSecret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", $"{serverId},d767d7d6-cc95-5579-bf32-a2ce31cc4660,*") }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Audience = "TeamSpeak Filetransfer",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key) { KeyId = "tsserver" }, SecurityAlgorithms.HmacSha512Signature)
         };

         tokenDescriptor.Claims = new Dictionary<string, object>();

         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/perm", new { getfile = $"{uuidHex}/{homeServer.Replace(".", "\\.")}/rooms/{roomId}/.*" });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/sq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/cq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/uq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/fs", 0);
         var token = tokenHandler.CreateToken(tokenDescriptor);
         return tokenHandler.WriteToken(token);
        }

        public static string GenerateUploadToken(string uuidHex, string serverId, string homeServer, string roomId)
        {
            var key = Convert.FromBase64String(Program.uploadTokenSecret);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
              Subject = new ClaimsIdentity(new[] {new Claim("sub", $"{serverId},d767d7d6-cc95-5579-bf32-a2ce31cc4660,*")}),
               Expires = DateTime.UtcNow.AddMinutes(5),
               Audience = "TeamSpeak Filetransfer",
               SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
         };

         tokenDescriptor.Claims = new Dictionary<string, object>();

         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/perm", new { putfile = $"{uuidHex}/{homeServer.Replace(".", "\\.")}/rooms/{roomId}/.*" });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/sq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/cq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/uq", new { read = -1, write = 0, store = 0 });
         tokenDescriptor.Claims.Add("http://v1.teamspeak.com/fs", 0);

         var token = tokenHandler.CreateToken(tokenDescriptor);
         return tokenHandler.WriteToken(token);
        }


        public static (string?, string?) ValidateUploadToken(string token){
            return ValidateToken(token, Convert.FromBase64String(Program.uploadTokenSecret));
        }

        public static (string?, string?) ValidateDownloadToken(string token){
            return ValidateToken(token, Convert.FromBase64String(Program.downloadTokenSecret));
        }

        public static (string?, string?) ValidateToken(string token, byte[] key)
        {
            if (token == null) 
               return (null, null);

            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                tokenHandler.ValidateToken(token, new TokenValidationParameters 
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = true,
                        ValidAudience = "TeamSpeak Filetransfer",
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var perms = jwtToken.Claims.First(x => x.Type == "http://v1.teamspeak.com/perm").Value;
                var sub = jwtToken.Claims.First(x => x.Type == "sub").Value;

                // return user id from JWT token if validation successful
                return (perms, sub);
            } catch {
                // return null if validation fails
                return (null, null);
            }
        }  

        public static (string?, string?) CheckUploadAuthorization(HttpListenerRequest request) {
            string? authHeader = request.Headers.Get("Authorization");
            if(authHeader == null){
                return (null, null);
            }
            if(!authHeader.StartsWith("Bearer ")){
                return (null, null);
            }
            string bearer = authHeader.Replace("Bearer ", "");
            if(String.IsNullOrEmpty(bearer)) {
                return (null, null);
            }
            return Token.ValidateUploadToken(bearer);
        }

        public static (string?, string?) CheckDownloadAuthorization(HttpListenerRequest request) {
            string? authHeader = request.Headers.Get("Authorization");
            if(authHeader == null){
                return (null, null);
            }
            if(!authHeader.StartsWith("Bearer ")){
                return (null, null);
            }
            string bearer = authHeader.Replace("Bearer ", "");
            if(String.IsNullOrEmpty(bearer)) {
                return (null, null);
            }
            return Token.ValidateDownloadToken(bearer);
        }
    }
}