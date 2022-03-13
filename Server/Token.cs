using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Server {
    public static class Token {


        public static string GenerateUploadToken(string sub) {
            return GenerateToken(sub, Convert.FromBase64String(Program.uploadTokenSecret));
        }

        public static string GenerateDownloadToken(string sub) {
            return GenerateToken(sub, Convert.FromBase64String(Program.downloadTokenSecret));
        }

        public static string GenerateToken(string sub, byte[] key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
              Subject = new ClaimsIdentity(new[] { new Claim("sub", sub) }),
               Expires = DateTime.UtcNow.AddHours(1),
               SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
         };
         var token = tokenHandler.CreateToken(tokenDescriptor);
         return tokenHandler.WriteToken(token);
        }


        public static string? ValidateUploadToken(string token){
            return ValidateToken(token, Convert.FromBase64String(Program.uploadTokenSecret));
        }

        public static string? ValidateDownloadToken(string token){
            return ValidateToken(token, Convert.FromBase64String(Program.downloadTokenSecret));
        }

        public static string? ValidateToken(string token, byte[] key)
        {
            if (token == null) 
               return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                tokenHandler.ValidateToken(token, new TokenValidationParameters 
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "sub").Value;

                // return user id from JWT token if validation successful
                return userId;
            } catch {
                // return null if validation fails
                return null;
            }
        }  

        public static string? CheckUploadAuthorization(HttpListenerRequest request) {
            string? authHeader = request.Headers.Get("Authorization");
            if(authHeader == null){
                return null;
            }
            if(!authHeader.StartsWith("Bearer ")){
                return null;
            }
            string bearer = authHeader.Replace("Bearer ", "");
            if(String.IsNullOrEmpty(bearer)) {
                return null;
            }
            return Token.ValidateUploadToken(bearer);
        }

        public static string? CheckDownloadAuthorization(HttpListenerRequest request) {
            string? authHeader = request.Headers.Get("Authorization");
            if(authHeader == null){
                return null;
            }
            if(!authHeader.StartsWith("Bearer ")){
                return null;
            }
            string bearer = authHeader.Replace("Bearer ", "");
            if(String.IsNullOrEmpty(bearer)) {
                return null;
            }
            return Token.ValidateDownloadToken(bearer);
        }
    }
}