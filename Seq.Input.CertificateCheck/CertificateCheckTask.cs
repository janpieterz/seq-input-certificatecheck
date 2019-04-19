using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Seq.Input.CertificateCheck
{
    internal class CertificateCheckTask : IDisposable
    {
        private readonly HttpClientWrapper _httpClientWrapper;
        readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        readonly Task _certificateCheckTask;
        public CertificateCheckTask(CertificateValidityCheck certificateCheck, TimeSpan interval, CertificateCheckReporter reporter, HttpClientWrapper httpClientWrapper, ILogger diagnosticLog)
        {
            if (certificateCheck == null) throw new ArgumentNullException(nameof(certificateCheck));
            if (reporter == null) throw new ArgumentNullException(nameof(reporter));
            _httpClientWrapper = httpClientWrapper;
            _certificateCheckTask =
                Task.Run(() => Run(certificateCheck, interval, reporter, _httpClientWrapper, diagnosticLog, _cancel.Token), _cancel.Token);
        }

        private static async Task Run(CertificateValidityCheck certificateCheck, TimeSpan interval, CertificateCheckReporter reporter, HttpClientWrapper httpClientWrapper, ILogger diagnosticLog, CancellationToken cancel)
        {
            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    var sw = Stopwatch.StartNew();
                    var result = await certificateCheck.CheckNow(httpClientWrapper, cancel, diagnosticLog).ConfigureAwait(false);
                    reporter.Report(result);
                    sw.Stop();
                    var total = sw.Elapsed.TotalMilliseconds;

                    if (total < interval.TotalMilliseconds)
                    {
                        var delay = (int)(interval.TotalMilliseconds - total);
                        await Task.Delay(delay, cancel).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Unloading
            }
            catch (Exception ex)
            {
                diagnosticLog.Fatal(ex, "The health check task threw an unhandled exception");
            }
        }

        public void Dispose()
        {
            _cancel.Dispose();
            _certificateCheckTask.Dispose();
        }

        public void Stop()
        {
            _cancel.Cancel();
            _certificateCheckTask.Wait();
        }
    }
}