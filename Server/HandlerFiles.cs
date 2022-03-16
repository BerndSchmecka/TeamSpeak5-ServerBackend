using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
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

                var endpointUrl = $"https://{Program.aws_bucket_name}.s3.{Program.aws_bucket_region}.amazonaws.com{Program.aws_base_path}{homeServer}/{serverId}/{roomId}/{uuidHex}/{fileName}";

                return new ResponseData(response, JsonSerializer.Serialize(new { 
                    location = endpointUrl,
                    headers = signAWSRequest("PUT", $"{homeServer}/{serverId}/{roomId}/{uuidHex}/{fileName}", "UNSIGNED-PAYLOAD")
                }), "application/json", 200);

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

                var endpointUrl = $"https://{Program.aws_bucket_name}.s3.{Program.aws_bucket_region}.amazonaws.com{Program.aws_base_path}{homeServer}/{serverId}/{roomId}/{uuidHex}/{fileName}";

try {
                HttpClient client = new HttpClient();
                Headers hd = signAWSRequest("GET", $"{homeServer}/{serverId}/{roomId}/{uuidHex}/{fileName}", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", hd.Authorization);
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-amz-content-sha256", hd.x_amz_content_sha256);
                client.DefaultRequestHeaders.TryAddWithoutValidation("x-amz-date", hd.x_amz_date);
                using(var res = await client.GetAsync(endpointUrl)) {
                    if(res.IsSuccessStatusCode) {
                                            using(var str = res.Content.ReadAsStream()){
                        byte[] array = new byte[str.Length];
                        str.Read(array, 0, array.Length);
                        return new ResponseData(response, array, System.Text.Encoding.UTF8, MimeTypeMap.GetMimeType(ext), 200);
                    }
                    } else if (res.StatusCode.Equals(HttpStatusCode.NotFound)){
                        return ApiServer.TS5ErrorData(response, "FILE_NOT_FOUND", "Not Found", 404);
                    } else {
                        return ApiServer.ErrorData(response, "Error getting file from AWS Bucket", (int)res.StatusCode);
                    }
                }
} catch (Exception ex) {
    return ApiServer.ErrorData(response, $"Error getting file: {ex.Message}", 500);
}
            } else {
                return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
            }
        }

        internal Headers signAWSRequest(string method, string path, string payloadHash){
                var date = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

                string canonicalRequest = $"{method}\n" +
                                          $"{Program.aws_base_path}{path}" + "\n" +
                                          "\n"+ 
                                          $"host:{Program.aws_bucket_name}.s3.{Program.aws_bucket_region}.amazonaws.com\n" +
                                          $"x-amz-content-sha256:{payloadHash}\n"+
                                          $"x-amz-date:{date}\n"+
                                          "\n"+
                                          "host;x-amz-content-sha256;x-amz-date\n"+
                                          $"{payloadHash}";

                string scope = DateTime.UtcNow.ToString("yyyyMMdd") + "/" + Program.aws_bucket_region + "/s3/aws4_request";
                string stringToSign = "AWS4-HMAC-SHA256" + "\n" + date + "\n" + scope + "\n" + BitConverter.ToString(Program.mySHA256.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest))).Replace("-", "").ToLower();

                HMACSHA256 DateKey = new HMACSHA256(Encoding.UTF8.GetBytes($"AWS4{Program.aws_secret_key}"));
                byte[] DateRegionKeyKey = DateKey.ComputeHash(Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("yyyyMMdd")));
                HMACSHA256 DateRegionKey = new HMACSHA256(DateRegionKeyKey);
                byte[] DateRegionServiceKeyKey = DateRegionKey.ComputeHash(Encoding.UTF8.GetBytes(Program.aws_bucket_region));
                HMACSHA256 DateRegionServiceKey = new HMACSHA256(DateRegionServiceKeyKey);
                byte[] SigningKeyKey = DateRegionServiceKey.ComputeHash(Encoding.UTF8.GetBytes("s3"));
                HMACSHA256 SigningKey = new HMACSHA256(SigningKeyKey);
                byte[] key = SigningKey.ComputeHash(Encoding.UTF8.GetBytes("aws4_request"));

                HMACSHA256 final = new HMACSHA256(key);
                byte[] signature = final.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

                string authHeader = $"AWS4-HMAC-SHA256 Credential={Program.aws_access_key}/{scope},SignedHeaders=host;x-amz-content-sha256;x-amz-date,Signature={BitConverter.ToString(signature).Replace("-", "").ToLower()}";

                return new Headers(authHeader, payloadHash, date);
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

    [Serializable]
    class Headers {
        [JsonPropertyName("Authorization")]
        public string Authorization {get; set;}

        [JsonPropertyName("x-amz-content-sha256")]
        public string x_amz_content_sha256 {get; set;}

        [JsonPropertyName("x-amz-date")]
        public string x_amz_date {get; set;}

        public Headers(string Authorization, string x_amz_content_sha256, string x_amz_date){
            this.Authorization = Authorization;
            this.x_amz_content_sha256 = x_amz_content_sha256;
            this.x_amz_date = x_amz_date;
        }
    }
}