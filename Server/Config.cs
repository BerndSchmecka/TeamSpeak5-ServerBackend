using System.Text.Json.Serialization;

namespace Server {

    [Serializable]
    public class Config {
        [JsonPropertyName("configVersion")]
        public int configVersion {get; set;}
        [JsonPropertyName("localHomeServer")]
        public string localHomeServer {get; set;}

        [JsonPropertyName("as_token")]
        public string as_token {get; set;}

        [JsonPropertyName("hs_token")]
        public string hs_token {get; set;}

        [JsonPropertyName("uploadTokenSecret")]
        public string uploadTokenSecret {get; set;}
        [JsonPropertyName("downloadTokenSecret")]
        public string downloadTokenSecret {get; set;}

        [JsonPropertyName("awsInfo")]
        public AWSInfo awsInfo {get; set;}
        public Config(int configVersion, string localHomeServer, string as_token, string hs_token, string uploadTokenSecret, string downloadTokenSecret, AWSInfo awsInfo){
            this.configVersion = configVersion;
            this.localHomeServer = localHomeServer;
            this.as_token = as_token;
            this.hs_token = hs_token;
            this.uploadTokenSecret = uploadTokenSecret;
            this.downloadTokenSecret = downloadTokenSecret;
            this.awsInfo = awsInfo;
        }
    }

    [Serializable]
    public class AWSInfo {
        [JsonPropertyName("bucketName")]
        public string bucketName {get; set;}

        [JsonPropertyName("bucketRegion")]
        public string bucketRegion {get; set;}

        [JsonPropertyName("basePath")]
        public string basePath {get; set;}

        [JsonPropertyName("accessKey")]
        public string accessKey {get; set;}

        [JsonPropertyName("secretKey")]
        public string secretKey {get; set;}

        public AWSInfo(string bucketName, string bucketRegion, string basePath, string accessKey, string secretKey){
            this.bucketName = bucketName;
            this.bucketRegion = bucketRegion;
            this.basePath = basePath;
            this.accessKey = accessKey;
            this.secretKey = secretKey;
        }
    }
}