using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Server {
    public class HandlerStorage : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            string path = request.Url.AbsolutePath;
            if(path.StartsWith("/storage/")) {
                (string?, string?) check = Token.CheckUploadAuthorization(request);

                string? perm = check.Item1;
                string? sub = check.Item2;

                if(perm == null || sub == null){
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 401);
                }
                Regex pathRegex = new Regex(Program.REGEX_STORAGE_PATH);
                if(!pathRegex.IsMatch(path)) {
                    return ApiServer.ErrorData(response, "invalid or malformed path", 400);
                }

                Match match = pathRegex.Match(path);
                string serverId = match.Groups[2].Value;
                string uuidHex = match.Groups[4].Value;
                string homeServer = match.Groups[1].Value;
                string roomId = match.Groups[3].Value;
                string fileName = match.Groups[5].Value;

                var permission = JsonSerializer.Deserialize<UploadPermResponse>(perm);

                Regex putfileReg = new Regex(Program.REGEX_PUTFILE);

                if(permission == null || permission.putfile == null || !putfileReg.IsMatch(permission.putfile)) {
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 401);
                }

                Match putfileMatch = putfileReg.Match(permission.putfile);
                string pfIdentity = putfileMatch.Groups[1].Value;
                string pfHomeServer = putfileMatch.Groups[2].Value.Replace("\\.", ".");
                string pfRoomId = putfileMatch.Groups[3].Value;
                string pfExt = putfileMatch.Groups[4].Value.Split('.').Last();

                string[] subsplit = sub.Split(",");

                string ext = fileName.Split('.').Last();

                if(subsplit.Length != 3 || !subsplit[0].Equals(serverId)){
                    return ApiServer.TS5ErrorData(response, "UNAUTHORIZED", "not authorized for this virtual server", 401);
                }

                //TODO: Check if User is in Room (roomId)

                if(!pfIdentity.Equals(uuidHex) || !pfHomeServer.Equals(homeServer) || !pfRoomId.Equals(Program.PUTFILE_ROOM_PLACEHOLDER/*roomId*/) || (!pfExt.Equals("*") && !pfExt.Equals(ext))) {
                    return ApiServer.TS5ErrorData(response, "UNAUTHORIZED", "unauthorized path", 401);
                }

                Directory.CreateDirectory(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex));

                using (Stream body = request.InputStream) {
                    using (Stream s = File.Create(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex, fileName))){
                        body.CopyTo(s);
                        return new ResponseData(response, "", "text/plain", 201);
                    }
                }
            } else {
                return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
            }
        }
    }
}