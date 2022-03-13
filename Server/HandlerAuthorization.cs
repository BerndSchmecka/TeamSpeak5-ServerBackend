using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Server {
    public class HandlerAuthorization : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            string path = request.Url.AbsolutePath;
            Regex downloadPath = new Regex(Program.REGEX_DOWNLOAD_TOKEN);
            if(path.Equals("/authorization/v1/matrix_uploadtoken")) {
                if(!request.HasEntityBody) {
                    return ApiServer.ErrorData(response, "Request body must not be empty", 400);
                }
                if(request.ContentType != "application/json") {
                    return ApiServer.ErrorData(response, "Request body must be of type application/json", 406);
                }
                using (System.IO.Stream body = request.InputStream) {
                    using (var reader = new System.IO.StreamReader(body, request.ContentEncoding)){
                        string bodyStr = await reader.ReadToEndAsync();
                        var obj = JsonSerializer.Deserialize<TokenRequest>(bodyStr);
                        if(obj.HomeServer == null){
                            return ApiServer.ErrorData(response, "HomeServer may not be null", 400);
                        }
                        if(obj.Token == null){
                            return ApiServer.ErrorData(response, "Token may not be null", 400);
                        }
                        if(!obj.HomeServer.Equals(Program.localHomeServer)){
                            return ApiServer.ErrorData(response, "user is not local", 401);
                        }
                        var res = await validateOpenIDToken(obj);
                        if(!res.IsSuccessStatusCode){
                            return new ResponseData(response, await res.Content.ReadAsStringAsync(), "application/json", (int)res.StatusCode);
                        }
                        var tokenResponse = JsonSerializer.Deserialize<VerifyTokenResponse>(await res.Content.ReadAsStringAsync());
                        Regex matrixRegEx = new Regex(Program.MATRIX_USER_IDENTIFIER_REGEX, RegexOptions.IgnoreCase);
                        if(!matrixRegEx.IsMatch(tokenResponse.sub)){
                            return ApiServer.ErrorData(response, "Internal error validating Matrix user", 500);
                        }
                        return new ResponseData(response, JsonSerializer.Serialize(new TokenResponse(Token.GenerateUploadToken(tokenResponse.sub))), "application/json", 200);
                    }
                }
            } else if (downloadPath.IsMatch(path)) {
                Match match = downloadPath.Match(path);
                string matrixId = match.Groups[1].Value;
                string roomId = match.Groups[1].Value;

                if(!request.HasEntityBody) {
                    return ApiServer.ErrorData(response, "Request body must not be empty", 400);
                }
                if(request.ContentType != "application/json") {
                    return ApiServer.ErrorData(response, "Request body must be of type application/json", 406);
                }
                using (System.IO.Stream body = request.InputStream) {
                    using (var reader = new System.IO.StreamReader(body, request.ContentEncoding)){
                        string bodyStr = await reader.ReadToEndAsync();
                        var obj = JsonSerializer.Deserialize<TokenRequest>(bodyStr);
                        if(obj.HomeServer == null){
                            return ApiServer.ErrorData(response, "HomeServer may not be null", 400);
                        }
                        if(obj.Token == null){
                            return ApiServer.ErrorData(response, "Token may not be null", 400);
                        }
                        if(!obj.HomeServer.Equals(Program.localHomeServer)){
                            return ApiServer.ErrorData(response, "user is not local", 401);
                        }
                        var res = await validateOpenIDToken(obj);
                        if(!res.IsSuccessStatusCode){
                            return new ResponseData(response, await res.Content.ReadAsStringAsync(), "application/json", (int)res.StatusCode);
                        }
                        var tokenResponse = JsonSerializer.Deserialize<VerifyTokenResponse>(await res.Content.ReadAsStringAsync());
                        Regex matrixRegEx = new Regex(Program.MATRIX_USER_IDENTIFIER_REGEX, RegexOptions.IgnoreCase);
                        if(!matrixRegEx.IsMatch(tokenResponse.sub)){
                            return ApiServer.ErrorData(response, "Internal error validating Matrix user", 500);
                        }
                        return new ResponseData(response, JsonSerializer.Serialize(new TokenResponse(Token.GenerateDownloadToken(tokenResponse.sub))), "application/json", 200);
                    }
                }
            } else {
                return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
            }
        }

        private async Task<HttpResponseMessage> validateOpenIDToken(TokenRequest req) {
            string url = $"https://{req.HomeServer}/_matrix/federation/v1/openid/userinfo?access_token={req.Token}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders
      .UserAgent
      .TryParseAdd(Program.displayableVersion);
            var result = await client.GetAsync(url);
            return result;
        }
    }

    [Serializable]
    class TokenRequest {
        [JsonPropertyName("HomeServer")]
        public string HomeServer {get; set;}

        [JsonPropertyName("Token")]
        public string Token {get; set;}
    }

    [Serializable]
    class VerifyTokenResponse {
        [JsonPropertyName("sub")]
        public string sub {get; set;}
    }

    [Serializable]
    class TokenResponse {
        [JsonPropertyName("token")]
        public string token {get; set;}

        public TokenResponse(string token){
            this.token = token;
        }
    }
}