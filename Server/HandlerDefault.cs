using System.Net;

namespace Server {
    public class HandlerDefault : Handler
    {
        public async Task<ResponseData> generateResponse(HttpListenerRequest request, HttpListenerResponse response)
        {
            return ApiServer.ErrorData(response, $"Cannot {request.HttpMethod} {request.Url.AbsolutePath}", 404);
        }
    }
}