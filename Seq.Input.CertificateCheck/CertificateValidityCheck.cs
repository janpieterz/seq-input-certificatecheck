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
        const string OutcomeSucceeded = "succeeded", OutcomeFailed = "failed";
        public CertificateValidityCheck(string title, string targetUrl, int validityDays)
        {
            _title = title ?? throw new ArgumentNullException(nameof(title));
            _targetUrl = targetUrl ?? throw new ArgumentNullException(nameof(targetUrl));
            _validityDays = validityDays;
        }

        public async Task<CertificateCheckResult> CheckNow(CancellationToken cancel)
        {
            string outcome;
            var utcTimestamp = DateTime.UtcNow;
            DateTime? expiresAtUtc = null;
            try
            {
                X509Certificate2 certificate = null;
                HttpClientHandler handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = delegate (HttpRequestMessage message, X509Certificate2 x509Certificate2, X509Chain arg3,
                        SslPolicyErrors arg4)
                    {
                        certificate = new X509Certificate2(x509Certificate2.RawData);
                        return true;
                    }
                };
                HttpClient client = new HttpClient(handler, true);
                await client.GetAsync(_targetUrl, cancel).ConfigureAwait(false);
                expiresAtUtc = certificate?.NotAfter;
                bool valid = expiresAtUtc.HasValue && expiresAtUtc.Value > DateTime.UtcNow.AddDays(_validityDays);
                outcome = valid ? OutcomeSucceeded : OutcomeFailed;
                client.Dispose();
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