using System;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace Server {
        public struct ResponseData {
            public HttpListenerResponse Response {get;}
            public byte[] Data {get;}
            public string ContentType {get;}
            public int StatusCode {get;}

        public ResponseData(HttpListenerResponse resp, string data_to_send, string contentType, int statusCode){
            this.Response = resp;
            this.Data = Encoding.UTF8.GetBytes(data_to_send);
            this.ContentType = contentType;
            this.StatusCode = statusCode;

            this.Response.ContentType = this.ContentType;
            this.Response.ContentEncoding = Encoding.UTF8;
            this.Response.ContentLength64 = this.Data.LongLength;
            this.Response.StatusCode = this.StatusCode;
        }

        public ResponseData(HttpListenerResponse resp, byte[] data_to_send, Encoding encoding, string contentType, int statusCode){
            this.Response = resp;
            this.Data = data_to_send;
            this.ContentType = contentType;
            this.StatusCode = statusCode;

            this.Response.ContentType = this.ContentType;
            this.Response.ContentEncoding = encoding;
            this.Response.ContentLength64 = this.Data.LongLength;
            this.Response.StatusCode = this.StatusCode;
        }
    }

    class ApiServer {
        private static string url = "http://*:8876/";

        private async Task Listen(string prefix, int maxConcurrentRequests, CancellationToken token){
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            var requests = new HashSet<Task>();
            for(int i=0; i < maxConcurrentRequests; i++)
                requests.Add(listener.GetContextAsync());

            while (!token.IsCancellationRequested){
                Task t = await Task.WhenAny(requests);
                requests.Remove(t);

                if (t is Task<HttpListenerContext>){
                    var context = (t as Task<HttpListenerContext>).Result;
                    requests.Add(HandleInboundConnections(context));
                    requests.Add(listener.GetContextAsync());
                }
            }
        }

        private async Task HandleInboundConnections(HttpListenerContext context) {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("X-Powered-By", Program.displayableVersion);

                ResponseData rd = new ResponseData(response, Program.ERROR_TEMPLATE("404 Not Found"), "text/html", 404);

                if(request.HttpMethod == "GET"){
                    Handler handler;
                    if(request.Url.AbsolutePath.StartsWith("/files/v1/file/")) {
                        handler = new HandlerFiles();
                    } else if (request.Url.AbsolutePath.StartsWith("/_matrix/app/v1/users/")) {
                        handler = new HandlerAppService();
                    } else {
                        handler = new HandlerDefault();
                    }

                    try {
                        rd = await handler.generateResponse(request, response);
                    } catch (Exception ex) {
                        rd = ApiServer.ErrorData(response, ex.Message, 500);
                    }
                } else if (request.HttpMethod == "OPTIONS"){
                    rd = new ResponseData(response, Program.ERROR_TEMPLATE("405 Method Not Allowed"), "text/html", 405);
                } else if (request.HttpMethod == "PUT") {
                    Handler handler;
                    if(request.Url.AbsolutePath.StartsWith("/_matrix/app/v1/transactions/")) {
                        handler = new HandlerAppService();
                    } else {
                        handler = new HandlerDefault();
                    }

                    try {
                        rd = await handler.generateResponse(request, response);
                    } catch (Exception ex) {
                        rd = ApiServer.ErrorData(response, ex.Message, 500);
                    }
                } else if (request.HttpMethod == "POST") {
                    Handler handler;
                    if(request.Url.AbsolutePath.StartsWith("/authorization/")) {
                        handler = new HandlerAuthorization();
                    } else if (request.Url.AbsolutePath.StartsWith("/files/v1/upload/")) {
                        handler = new HandlerFiles();
                    } else {
                        handler = new HandlerDefault();
                    }

                    try {
                        rd = await handler.generateResponse(request, response);
                    } catch (Exception ex) {
                        rd = ApiServer.ErrorData(response, ex.Message, 500);
                    }
                } else {
                    rd = new ResponseData(response, Program.ERROR_TEMPLATE("405 Method Not Allowed"), "text/html", 405);
                }
                
                await response.OutputStream.WriteAsync(rd.Data, 0, rd.Data.Length);
                response.Close();
        }

        public void StartServer(){
            CancellationToken token = new CancellationToken();
            Task listenTask = Listen(url, 32, token);
            listenTask.GetAwaiter().GetResult();
        }

        public static ResponseData ErrorData(HttpListenerResponse response, string errorMessage, int errorCode) {
            return new ResponseData(response, "{\"errorMessage\": \"" + errorMessage +"\", \"errorCode\": " + errorCode + "}", "application/json", errorCode);
        }

        public static ResponseData TS5ErrorData(HttpListenerResponse response, string error, string message, int errorCode) {
            return new ResponseData(response, "{\"error\": \"" + error + "\", \"message\": \"" + message + "\"}", "application/json", errorCode);
        }
    }
}