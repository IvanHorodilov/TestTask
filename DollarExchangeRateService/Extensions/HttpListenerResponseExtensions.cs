using System.Net;

namespace DollarExchangeRateService.Extensions
{
    internal static class HttpListenerResponseExtensions
    {
        public static void ReturnNotFound(this HttpListenerResponse response, string responseString)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }

        public static void ReturnResponse(this HttpListenerResponse response, string responseString)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }

        public static void ReturnInternalServerError(this HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Close();
        }
    }
}
