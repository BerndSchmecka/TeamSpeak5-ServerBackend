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

        public Config(int configVersion, string localHomeServer, string uploadTokenSecret, string downloadTokenSecret){
            this.configVersion = configVersion;
            this.localHomeServer = localHomeServer;
            this.uploadTokenSecret = uploadTokenSecret;
            this.downloadTokenSecret = downloadTokenSecret;
        }
    }
}