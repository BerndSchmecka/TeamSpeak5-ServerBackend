using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TSLib;
using TSLib.Commands;
using TSLib.Full;
using TSLib.Messages;

namespace PokeBot {

    public static class JsonFileReader
    {
        public static T Read<T>(string filePath)
        {
            string text = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<T>(text);
        }
    }

    public class Program {

        public static TsFullClient client;

        public static string serverId = "";
        public static string serverAddress = "";
        public static string administrativeDomain = "";
        public static string chatTokenSecret = "";
        public static string clientIdentity = "";

        public static void Main(string[] args) {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            Console.CancelKeyPress += delegate {
                ConsoleEventCallback(2);
            };

            client = new TsFullClient();

            #if DEBUG
                System.Console.WriteLine("Mode=Debug");
            #else
                System.Console.WriteLine("Mode=Release");
            #endif

            Config cfg = JsonFileReader.Read<Config>("config.json");
            if(cfg.configVersion != 1) {
                Console.WriteLine("Wrong config version, exiting ...");
                return;
            }

            Program.serverId = cfg.serverId;
            Program.serverAddress = cfg.serverAddress;
            Program.administrativeDomain = cfg.administrativeDomain;
            Program.chatTokenSecret = cfg.chatTokenSecret;
            Program.clientIdentity = cfg.clientIdentity;

            Console.WriteLine($"serverId={serverId}");
            Console.WriteLine($"serverAddress={serverAddress}");
            Console.WriteLine($"administrativeDomain={administrativeDomain}");
            Console.WriteLine($"chatTokenSecret={chatTokenSecret.Substring(0, 5) + " ..."}");
            Console.WriteLine($"clientIdentity={clientIdentity.Substring(0, 5) + " ..."}");

            ConnectionDataFull cd = new ConnectionDataFull();
            cd.Address = Program.serverAddress;
            cd.Username = "Morpheus Test";
            cd.Identity = TsCrypt.LoadIdentityDynamic(clientIdentity).Value;
            cd.VersionSign = new VersionSign("5.0.0-qa-request-chat-7 [Build: 1646854452]", "Windows", "0iRGPzh37MelxPeKE15K754jAi+yXJ2bu+pXV1ErOFbEF504WtUZaAIoFVHBNheIVdSCrxuWFF17xG1w2gSICQ==");

            client.Connect(cd);

            client.OnClientPoke += (s, e) => {
                if(s is TsFullClient) {
                    var sender = (TsFullClient)s;
                    foreach (var poke in e) {
                        var uuidBase = poke.InvokerUid.ToString();
                        var uuidHex = BitConverter.ToString(Convert.FromBase64String(uuidBase)).Replace("-", "").ToLower();
                        var token = Token.GenerateChatToken(uuidHex);

                        sender.SendPrivateMessage(token, poke.InvokerId);
                        sender.SendPrivateMessage(Token.getChatConfig(token), poke.InvokerId);
                    }
                }
            };
        }

        static bool ConsoleEventCallback(int eventType) {
            if (eventType == 2) {
                Console.WriteLine("Console window closing, death imminent");
                if(client.Connected){
                    client.Disconnect();
                }
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);


        public static string getGZipTemplate(MatrixLoginResponse resp) => "{\"serial\":13,\"next_batch\":\"\",\"rooms\":{},\"pinned_messages\":{},\"room_invites\":{},\"presences\":{},\"login_data\":{\"user_id\":\"" + resp.user_id + "\",\"access_token\":\"" + resp.access_token + "\",\"home_server\":\"" + resp.home_server + "\",\"device_id\":\"" + resp.device_id + "\"},\"account_data\":{\"direct_rooms\":{\"rooms\":{}}},\"transaction_id\":1,\"ignored_users\":{},\"room_events\":{}}";

            public static string CompressString(string text)
        {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        var outputBytes = memoryStream.ToArray();
        var outputbase64 = Convert.ToBase64String(outputBytes);

        return outputbase64; // RETURNS AS BASE64
        //return Encoding.UTF8.GetString(gZipBuffer); // RETURN AS UTF8 STRING
        }
    }
}