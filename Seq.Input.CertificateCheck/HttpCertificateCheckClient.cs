using System;
using System.Net.Http;

namespace Seq.Input.CertificateCheck
{
    public static class HttpCertificateCheckClient
    {
        public static HttpClient Create(Action<DateTime> certificateExpiration)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) =>
                {
                    certificateExpiration(certificate2.NotAfter);
                    return true;
                }
            };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Connection.Add("Close");
            return httpClient;
        }
    }
}