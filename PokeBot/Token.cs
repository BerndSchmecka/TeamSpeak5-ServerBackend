using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace PokeBot {
    public static class Token {
        
        public static string GenerateChatToken(string uuidHex)
        {
            var key = Convert.FromBase64String(Program.chatTokenSecret);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
              Subject = new ClaimsIdentity(new[] {new Claim("sub", $"ts_{uuidHex}_{Program.serverId}")}),
               Expires = DateTime.UtcNow.AddMinutes(5),
               Audience = Program.serverId,
               SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

         var token = tokenHandler.CreateToken(tokenDescriptor);
         return tokenHandler.WriteToken(token);
        }

        /*public static string getChatConfig(string login_token) {
            string? wellKnownJson = null;
            try {
                WebRequest wc = HttpWebRequest.Create($"http://{Program.administrativeDomain}/.well-known/teamspeak/{Program.serverId}");
                var resp = wc.GetResponse();
                var str = new StreamReader(resp.GetResponseStream());
                wellKnownJson = str.ReadToEnd();
            } catch (Exception ex) {
                System.Console.WriteLine($"Error getting Well-Known from serverId: {ex.Message}");
                System.Console.WriteLine("Trying once more with default UUID ...");

                try {
                    WebRequest wc = HttpWebRequest.Create($"http://{Program.administrativeDomain}/.well-known/teamspeak/4ea3a86d-c671-5846-931d-15cce4f8501c");
                    var resp = wc.GetResponse();
                    var str = new StreamReader(resp.GetResponseStream());
                    wellKnownJson = str.ReadToEnd();
                } catch (Exception fx) {
                    System.Console.WriteLine($"Error getting Well-Known from default UUID: {fx.Message}");
                    System.Console.WriteLine("Aborting ...");

                    return "An internal error occured, please contact the owner of this server!";
                }
            }
            
            WellKnownConfig? wkc = null;
            try {
                wkc = JsonSerializer.Deserialize<WellKnownConfig>(wellKnownJson);
            } catch (Exception ex) {
                System.Console.WriteLine($"Error deserializing Well-Known-Config: {ex.Message}");
                return "An internal error occured, please contact the owner of this server!";
            }
            
            if(wkc == null){
                System.Console.WriteLine($"Error deserializing Well-Known-Config: wkc == null");
                return "An internal error occured, please contact the owner of this server!";
            }

            string matrixServer = wkc.chat.domain;

            string? matrixLoginInfo = null;
            try {
                WebRequest wc = HttpWebRequest.Create($"{matrixServer}/_matrix/client/v3/login");
                wc.Method = "POST";
                wc.ContentType = "application/json";
                using(var sw = new StreamWriter(wc.GetRequestStream())){
                    sw.Write("{\"type\": \"org.matrix.login.jwt\", \"token\": \"" + login_token + "\"}");
                }
                
                var resp = wc.GetResponse();
                var str = new StreamReader(resp.GetResponseStream());
                matrixLoginInfo = str.ReadToEnd();
            } catch (Exception ex) {
                System.Console.WriteLine($"Error during Matrix Login: {ex.Message}");
                return "An internal error occured, please contact the owner of this server!";
            }

            MatrixLoginResponse? response = null;
            try {
                response = JsonSerializer.Deserialize<MatrixLoginResponse>(matrixLoginInfo);
            } catch (Exception ex) {
                System.Console.WriteLine($"Error deserializing Matrix Login Response: {ex.Message}");
                return "An internal error occured, please contact the owner of this server!";
            }

            if(response == null){
                System.Console.WriteLine($"Error deserializing Matrix Login Response: response == null");
                return "An internal error occured, please contact the owner of this server!";
            }

            string rawStr = Program.getGZipTemplate(response);

            return Program.CompressString(rawStr);
        }*/
    }

    /*[Serializable]
    class WellKnownConfig {
        [JsonPropertyName("teamspeak.server")]
        public BaseUrlObj teamspeak_server {get; set;}

        [JsonPropertyName("chat")]
        public DomainObj chat {get; set;}

        [JsonPropertyName("files")]
        public BaseUrlObj files {get; set;}

        [JsonPropertyName("authorization")]
        public BaseUrlObj authorization {get; set;}

        public WellKnownConfig(BaseUrlObj teamspeak_server, DomainObj chat, BaseUrlObj files, BaseUrlObj authorization) {
            this.teamspeak_server = teamspeak_server;
            this.chat = chat;
            this.files = files;
            this.authorization = authorization;
        }
    }

    [Serializable]
    class BaseUrlObj {
        [JsonPropertyName("base_url")]
        public string base_url {get; set;}

        public BaseUrlObj(string base_url) => this.base_url = base_url;
    }

    [Serializable]
    class DomainObj {
        [JsonPropertyName("domain")]
        public string domain {get; set;}

        public DomainObj(string domain) => this.domain = domain;
    }

    [Serializable]
    public class MatrixLoginResponse {
        [JsonPropertyName("access_token")]
        public string access_token {get; set;}

        [JsonPropertyName("device_id")]
        public string device_id {get; set;}

        [JsonPropertyName("home_server")]
        public string home_server {get; set;}

        [JsonPropertyName("user_id")]
        public string user_id {get; set;}

        public MatrixLoginResponse(string access_token, string device_id, string home_server, string user_id){
            this.access_token = access_token;
            this.device_id = device_id;
            this.home_server = home_server;
            this.user_id = user_id;
        }
    }*/
}