using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Seq.Input.CertificateCheck
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private DateTime? _lastExpiration;
        public HttpClientWrapper()
        {
            _semaphore = new SemaphoreSlim(1);
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) =>
                {
                    _lastExpiration = certificate2.NotAfter;
                    return true;
                }
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Connection.Add("Close");
        }

        public async Task<DateTime?> CheckEndpoint(Uri endpoint, CancellationToken cancel)
        {
            await _semaphore.WaitAsync(cancel).ConfigureAwait(false);
            await _httpClient.GetAsync(endpoint, cancel).ConfigureAwait(false);
            var result = _lastExpiration;
            _lastExpiration = null;
            _semaphore.Release();
            return result;
        }
    }
}