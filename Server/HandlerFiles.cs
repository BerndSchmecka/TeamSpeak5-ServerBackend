using System.Net;
using System.Text.RegularExpressions;
using MimeTypes;

namespace Server {
    public class HandlerFiles : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            string path = request.Url.AbsolutePath;
            if(path.StartsWith("/files/v1/upload/")) {
                string? sub = Token.CheckUploadAuthorization(request);
                if(sub == null){
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 400);
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

                Directory.CreateDirectory(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex));


                /* TODO 
                {"location": "", "headers": ""}
                
                */

                return new ResponseData(response, Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex), "text/plain", 200);
            } else if(path.StartsWith("/files/v1/file/")) {
                string? sub = Token.CheckDownloadAuthorization(request);
                if(sub == null){
                    return ApiServer.TS5ErrorData(response, "TOKEN_MISSING", "invalid or malformed auth method", 400);
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

                string ext = fileName.Split('.').Last();

                Directory.CreateDirectory(Path.Combine("FileStorage", homeServer, serverId, roomId, uuidHex));
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
}