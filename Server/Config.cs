using System.Text.Json.Serialization;

namespace Server {

    [Serializable]
    public class Config {
        [JsonPropertyName("configVersion")]
        public int configVersion {get; set;}
        [JsonPropertyName("localHomeServer")]
        public string localHomeServer {get; set;}
        [JsonPropertyName("uploadTokenSecret")]
        public string uploadTokenSecret {get; set;}
        [JsonPropertyName("downloadTokenSecret")]
        public string downloadTokenSecret {get; set;}

        [JsonPropertyName("awsInfo")]
        public AWSInfo awsInfo {get; set;}

        public Config(int configVersion, string localHomeServer, string uploadTokenSecret, string downloadTokenSecret, AWSInfo awsInfo){
            this.configVersion = configVersion;
            this.localHomeServer = localHomeServer;
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