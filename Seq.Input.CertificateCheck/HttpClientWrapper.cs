using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Seq.Input.CertificateCheck.Extensions;

namespace Seq.Input.CertificateCheck
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore;
        private DateTime? _lastExpiration;
        private string _issuer;
        private string _subject;
        private string _thumbprint;
        private string _serialNumber;
        private IEnumerable<string> _subjectAlternativeNames;

        public HttpClientWrapper()
        {
            _semaphore = new SemaphoreSlim(1);
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) =>
                {
                    _lastExpiration = certificate2.NotAfter;
                    _issuer = certificate2.Issuer;
                    _subject = certificate2.Subject;
                    _thumbprint = certificate2.Thumbprint?.ToLower();
                    _serialNumber = certificate2.SerialNumber;
                    _subjectAlternativeNames = certificate2.ParseSubjectAlternativeNames();
                    return true;
                }
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Connection.Add("Close");
        }

        public async Task<CertificateInformation> CheckEndpoint(Uri endpoint, CancellationToken cancel)
        {
            await _semaphore.WaitAsync(cancel).ConfigureAwait(false);
            try
            {
                await _httpClient.GetAsync(endpoint, cancel).ConfigureAwait(false);
                var result = new CertificateInformation
                {
                    Subject = _subject,
                    Issuer = _issuer,
                    LastExpiration = _lastExpiration,
                    Thumbprint = _thumbprint,
                    SerialNumber = _serialNumber,
                    SubjectAlternativeNames = _subjectAlternativeNames,
                };
                _lastExpiration = null;
                return result;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}