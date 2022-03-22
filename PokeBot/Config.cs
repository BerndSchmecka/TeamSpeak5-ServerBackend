using System.Text.Json.Serialization;

namespace PokeBot {

    [Serializable]
    public class Config {
        [JsonPropertyName("configVersion")]
        public int configVersion {get; set;}

        [JsonPropertyName("serverId")]
        public string serverId {get; set;}

        [JsonPropertyName("serverAddress")]
        public string serverAddress {get; set;}

        [JsonPropertyName("serverPassword")]
        public string? serverPassword {get; set;}

        /*[JsonPropertyName("administrativeDomain")]
        public string administrativeDomain {get; set;}*/

        [JsonPropertyName("chatTokenSecret")]
        public string chatTokenSecret {get; set;}

        [JsonPropertyName("clientIdentity")]
        public string clientIdentity {get; set;}

        public Config(int configVersion, string serverId, string serverAddress, string? serverPassword, /*string administrativeDomain,*/ string chatTokenSecret, string clientIdentity) {
            this.configVersion = configVersion;
            this.serverId = serverId;
            this.serverAddress = serverAddress;
            this.serverPassword = serverPassword;
            //this.administrativeDomain = administrativeDomain;
            this.chatTokenSecret = chatTokenSecret;
            this.clientIdentity = clientIdentity;
        }
    }
}