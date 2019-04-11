using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Seq.Input.CertificateCheck
{
    public static class HttpCertificateCheckClient
    {
        public static HttpClient Create(Action<Uri, X509Certificate2> certificateCallback)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false, ServerCertificateCustomValidationCallback = delegate (HttpRequestMessage message, X509Certificate2 x509Certificate2, X509Chain arg3,
                    SslPolicyErrors arg4)
                {
                    var certificate = new X509Certificate2(x509Certificate2.RawData);
                    certificateCallback(message.RequestUri, certificate);
                    return true;
                }
            };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Connection.Add("Close");
            return httpClient;
        }
    }
}