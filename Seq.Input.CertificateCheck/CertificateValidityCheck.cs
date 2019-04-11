using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Seq.Input.CertificateCheck
{
    public class CertificateValidityCheck
    {
        private readonly string _title;
        private readonly string _targetUrl;
        private readonly int _validityDays;
        private readonly HttpClient _httpClient;
        private readonly Func<Uri, X509Certificate2> _certificateLookup;
        const string OutcomeSucceeded = "succeeded", OutcomeFailed = "failed";
        public CertificateValidityCheck(string title, string targetUrl, int validityDays, HttpClient httpClient, Func<Uri, X509Certificate2> certificateLookup)
        {
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _targetUrl = targetUrl ?? throw new ArgumentNullException(nameof(targetUrl));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _validityDays = validityDays;
            _httpClient = httpClient;
            _certificateLookup = certificateLookup;
        }

        public async Task<CertificateCheckResult> CheckNow(CancellationToken cancel)
        {
            string outcome;
            var utcTimestamp = DateTime.UtcNow;
            DateTime? expiresAtUtc = null;
            try
            {
                var result = await _httpClient.GetAsync(_targetUrl, cancel).ConfigureAwait(false);
                var certificate = _certificateLookup(result.RequestMessage.RequestUri);
                expiresAtUtc = certificate?.NotAfter;
                bool valid = expiresAtUtc.HasValue && expiresAtUtc.Value > DateTime.UtcNow.AddDays(_validityDays);
                outcome = valid ? OutcomeSucceeded : OutcomeFailed;
            }
            catch (Exception)
            {
                outcome = OutcomeFailed;
            }

            var level = outcome == OutcomeFailed ? "Error" :
                null;

            return new CertificateCheckResult(utcTimestamp, _title, _targetUrl, outcome, level, expiresAtUtc);
        }
    }
}