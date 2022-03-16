using System;
using System.Text.Json;

namespace Server
{

    public static class JsonFileReader
    {
        public static T Read<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }
    }

    class Program
    {
        public static string ERROR_TEMPLATE(string error_message) {return String.Format("<html><head><title>{0}</title></head><body><center><h1>{0}</h1></center><hr><center>{1}</center></body></html>", error_message, displayableVersion);}

        public static Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        public static DateTime buildDate = new DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.Revision * 2);
        public static string displayableVersion = null;

        public static DateTime epoch = new System.DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);

        public static string TS5_MATRIX_USER_IDENTIFIER_REGEX = @"@ts_([0-9a-fA-F]{40})_([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}):([A-Za-z0-9.-]+\.[A-Za-z]{2,})";
        public static string REGEX_UPLOAD_PATH = @"\/files\/v1\/upload\/([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\/chan\/([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\/([a-fA-F0-9]{40})\/([A-Za-z0-9.]+)\/rooms\/([A-Za-z]{18})\/([^/]*$)";
        public static string REGEX_DOWNLOAD_PATH = @"\/files\/v1\/file\/([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\/chan\/([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\/([a-fA-F0-9]{40})\/([A-Za-z0-9.]+)\/rooms\/([A-Za-z]{18})\/([^/]*$)";
        public static string REGEX_DOWNLOAD_TOKEN = @"\/authorization\/v1\/matrix_downloadtoken\/((%40|@)[A-Za-z0-9._=-]+(%3A|:)[A-Za-z0-9.-]+\.[A-Za-z]{2,})\/((%21|!)([A-Za-z0-9._=-]{18})(%3A|:)[A-Za-z0-9.-]+\.[A-Za-z]{2,})";
        public static string REGEX_GETFILE = @"([0-9A-Fa-f]{40})\/([A-Za-z0-9\\.]+)\/rooms\/([A-Za-z]{18})\/([^/]*$)";
        public static string REGEX_PUTFILE = @"([0-9A-Fa-f]{40})\/([A-Za-z0-9\\.]+)\/rooms\/([A-Za-z]{18})\/([^/]*$)";
        public static string REGEX_STORAGE_PATH = @"\/storage\/([A-Za-z0-9.]+)\/([a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\/([A-Za-z]{18})\/([a-fA-F0-9]{40})\/([^/]*$)";
        public static string PUTFILE_ROOM_PLACEHOLDER = "aaaaaaaaaaaaaaaaaa";

        internal static string localHomeServer = "";
        internal static string uploadTokenSecret = "";
        internal static string downloadTokenSecret = "";
        internal static string aws_base_url = "";
        internal static string aws_access_id = "";
        internal static string aws_secret_key = "";

        static void Main(string[] args)
        {
            #if DEBUG
                System.Console.WriteLine("Mode=Debug");
                displayableVersion = $"TS5-ServerBackend/{version} ({buildDate}; by Dunkelmann) RuntimeMode/0 (staging)";
            #else
                System.Console.WriteLine("Mode=Release");
                displayableVersion = $"TS5-ServerBackend/{version} ({buildDate}; by Dunkelmann) RuntimeMode/1 (production)";
            #endif

            Config cfg = JsonFileReader.Read<Config>("config.json");
            if(cfg.configVersion != 2) {
                Console.WriteLine("Wrong config version, exiting ...");
                return;
            }
            Program.localHomeServer = cfg.localHomeServer;
            Program.uploadTokenSecret = cfg.uploadTokenSecret;
            Program.downloadTokenSecret = cfg.downloadTokenSecret;

            Program.aws_base_url = cfg.awsInfo.base_url;
            Program.aws_access_id = cfg.awsInfo.accessId;
            Program.aws_secret_key = cfg.awsInfo.secretKey;

            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"localHomeServer={localHomeServer}");
            Console.WriteLine($"uploadTokenSecret={uploadTokenSecret.Substring(0, 5) + " ..."}");
            Console.WriteLine($"downloadTokenSecret={downloadTokenSecret.Substring(0, 5) + " ..."}");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine($"awsBaseUrl={aws_base_url}");
            Console.WriteLine($"awsAccessId={aws_access_id}");
            Console.WriteLine($"awsSecretKey={aws_secret_key.Substring(0, 5) + " ..."}");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Starting server ...");
            ApiServer server = new ApiServer();
            server.StartServer();
        }
    }
}
