using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Server {
    public class HandlerAppService : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            if(!CheckHomeServerAuthorization(request)){
                return ApiServer.ErrorData(response, "M_UNAUTHORIZED", 401);
            }

            string path = request.Url.AbsolutePath;
            if(path.StartsWith("/_matrix/app/v1/transactions/")) {
                if(!request.HasEntityBody) {
                    return ApiServer.ErrorData(response, "Request body must not be empty", 400);
                }
                if(request.ContentType != "application/json") {
                    return ApiServer.ErrorData(response, "Request body must be of type application/json", 406);
                }
                using (System.IO.Stream body = request.InputStream) {
                    using (var reader = new System.IO.StreamReader(body, request.ContentEncoding)){
                        string bodyStr = await reader.ReadToEndAsync();
                        //TODO - Implement Transactions - return 200 OK for now
                        return new ResponseData(response, "{}", "application/json", 200);
                    }
                }
            } else if(path.StartsWith("/_matrix/app/v1/users/")) {
                Regex usersRegex = new Regex(Program.REGEX_APPSERVICE_USERS);
                if(!usersRegex.IsMatch(path)){
                    return ApiServer.ErrorData(response, "M_NOT_FOUND", 404);
                }
                Match match = usersRegex.Match(path);
                string uuidHex = match.Groups[1].Value;
                string serverUuid = match.Groups[2].Value;
                string homeServer = match.Groups[3].Value;
                if(!homeServer.Equals(Program.localHomeServer)){
                    return ApiServer.ErrorData(response, "M_NOT_FOUND", 404);
                }

                if(await createMatrixUser(uuidHex, serverUuid, homeServer)){
                    return new ResponseData(response, "{}", "application/json", 200);
                } else {
                    return ApiServer.ErrorData(response, "M_NOT_FOUND", 404);
                }
            } else {
                return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
            }
        }

        public static bool CheckHomeServerAuthorization(HttpListenerRequest request) {
            var query = request.QueryString;
            var token = query.Get("access_token");
            if(token == null){
                return false;
            }
            return token.Equals(Program.homeServerToken);
        }

        internal static async Task<bool> createMatrixUser(string uuidHex, string serverUuid, string homeServer){
            string url = $"https://{homeServer}/_matrix/client/v3/register?access_token={Program.appServiceToken}";
            using var client = new HttpClient();
            client.DefaultRequestHeaders
      .UserAgent
      .TryParseAdd(Program.displayableVersion);

      var content = new StringContent("{\"type\":\"m.login.application_service\",\"username\":\"" + $"ts_{uuidHex}_{serverUuid}" +"\"}", Encoding.UTF8, "application/json");

            var result = await client.PostAsync(url, content);
            return result.StatusCode == HttpStatusCode.OK;
        }
    }
}