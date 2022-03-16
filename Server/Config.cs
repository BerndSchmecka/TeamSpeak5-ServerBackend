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
        [JsonPropertyName("base_url")]
        public string base_url {get; set;}

        [JsonPropertyName("accessId")]
        public string accessId {get; set;}

        [JsonPropertyName("secretKey")]
        public string secretKey {get; set;}

        public AWSInfo(string base_url, string accessId, string secretKey){
            this.base_url = base_url;
            this.accessId = accessId;
            this.secretKey = secretKey;
        }
    }
}