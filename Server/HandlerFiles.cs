using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MimeTypes;

namespace Server {
    public class HandlerFiles : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            string path = request.Url.AbsolutePath;
            if(path.StartsWith("/files/v1/upload/")) {
                (string?, string?) check = Token.CheckUploadAuthorization(request);

                string? perm = check.Item1;
                string? sub = check.Item2;

                if(perm == null || sub == null){
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 401);
                }
                Regex pathRegex = new Regex(Program.REGEX_UPLOAD_PATH);
                if(!pathRegex.IsMatch(path)) {
                    return ApiServer.ErrorData(response, "invalid or malformed path", 400);
                }

                Match match = pathRegex.Match(path);
                string serverId = match.Groups[1].Value;
                //string channelId = match.Groups[2].Value;
                string uuidHex = match.Groups[3].Value;
                string homeServer = match.Groups[4].Value;
                string roomId = match.Groups[5].Value;
                string fileName = match.Groups[6].Value;

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

                return new ResponseData(response, JsonSerializer.Serialize(new { location = $"https://{request.UserHostName}/storage/{homeServer}/{serverId}/{roomId}/{uuidHex}/{fileName}", headers = new { Authorization = request.Headers.Get("Authorization") }}), "application/json", 200);
            } else if(path.StartsWith("/files/v1/file/")) {
                (string?, string?) check = Token.CheckDownloadAuthorization(request);
                string? perm = check.Item1;
                string? sub = check.Item2;

                if(perm == null || sub == null){
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 401);
                }
                Regex pathRegex = new Regex(Program.REGEX_DOWNLOAD_PATH);
                if(!pathRegex.IsMatch(path)) {
                    return ApiServer.ErrorData(response, "invalid or malformed path", 400);
                }

                Match match = pathRegex.Match(path);
                string serverId = match.Groups[1].Value;
                //string channelId = match.Groups[2].Value;
                string uuidHex = match.Groups[3].Value;
                string homeServer = match.Groups[4].Value;
                string roomId = match.Groups[5].Value;
                string fileName = match.Groups[6].Value;

                var permission = JsonSerializer.Deserialize<DownloadPermResponse>(perm);

                Regex getfileReg = new Regex(Program.REGEX_GETFILE);

                if(permission == null || permission.getfile == null || !getfileReg.IsMatch(permission.getfile)) {
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 401);
                }

                Match getfileMatch = getfileReg.Match(permission.getfile);
                string gfIdentity = getfileMatch.Groups[1].Value;
                string gfHomeServer = getfileMatch.Groups[2].Value.Replace("\\.", ".");
                string gfRoomId = getfileMatch.Groups[3].Value;
                string gfExt = getfileMatch.Groups[4].Value.Split('.').Last();

                string[] subsplit = sub.Split(",");

                string ext = fileName.Split('.').Last();

                if(subsplit.Length != 3 || !subsplit[0].Equals(serverId)){
                    return ApiServer.TS5ErrorData(response, "UNAUTHORIZED", "not authorized for this virtual server", 401);
                }

                if(!gfIdentity.Equals(uuidHex) || !gfHomeServer.Equals(homeServer) || !gfRoomId.Equals(roomId) || (!gfExt.Equals("*") && !gfExt.Equals(ext))) {
                    return ApiServer.TS5ErrorData(response, "UNAUTHORIZED", "unauthorized path", 401);
                }

                Directory.CreateDirectory(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex));

                if(!File.Exists(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex, fileName))){
                    return ApiServer.TS5ErrorData(response, "FILE_NOT_FOUND", "Not Found", 404);
                }

                using(var fs = File.OpenRead(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex, fileName))) {
                    byte[] array = new byte[fs.Length];
                    fs.Read(array, 0, array.Length);
                    return new ResponseData(response, array, System.Text.Encoding.UTF8, MimeTypeMap.GetMimeType(ext), 200);
                }
            } else {
                return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
            }
        }
    }

    [Serializable]
    class DownloadPermResponse {
        [JsonPropertyName("getfile")]
        public string getfile {get; set;}

        public DownloadPermResponse(string getfile){
            this.getfile = getfile;
        }
    }

    [Serializable]
    class UploadPermResponse {
        [JsonPropertyName("putfile")]
        public string putfile {get; set;}

        public UploadPermResponse(string putfile){
            this.putfile = putfile;
        }
    }
}