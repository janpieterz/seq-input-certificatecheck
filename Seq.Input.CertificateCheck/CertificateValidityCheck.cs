using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

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

        public async Task<CertificateCheckResult> CheckNow(HttpClientWrapper httpClientWrapper, CancellationToken cancel, ILogger diagnosticLog)
        {
            string outcome;
            var utcTimestamp = DateTime.UtcNow;
            DateTime? expiresAtUtc = null;
            try
            {
                expiresAtUtc = await httpClientWrapper.CheckEndpoint(new Uri(_targetUrl), cancel).ConfigureAwait(false);
                bool valid = expiresAtUtc.HasValue && expiresAtUtc.Value > DateTime.UtcNow.AddDays(_validityDays);
                outcome = valid ? OutcomeSucceeded : OutcomeFailed;
            }
            catch (Exception exception)
            {
                diagnosticLog.Error(exception, "Something went wrong while checking certificate");
                outcome = OutcomeFailed;
            }

            var level = outcome == OutcomeFailed ? "Error" :
                null;

            return new CertificateCheckResult(utcTimestamp, _title, _targetUrl, outcome, level, expiresAtUtc);
        }
    }
}