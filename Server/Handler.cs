using System.Net;

namespace Server {
    interface Handler {
        public Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response);
    }
}