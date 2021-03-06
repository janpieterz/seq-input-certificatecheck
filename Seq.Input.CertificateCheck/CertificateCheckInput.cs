﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Seq.Apps;

namespace Seq.Input.CertificateCheck
{
    [SeqApp("Certificate Check Input",
        Description = "Periodically get endpoint certificates, validate their expiration and publish results to Seq.")]
    public class CertificateCheckInput : SeqApp, IPublishJson, IDisposable 
    {
        private readonly List<CertificateCheckTask> _certificateCheckTasks = new List<CertificateCheckTask>();
        private HttpClientWrapper _httpClientWrapper;
        [SeqAppSetting(
            DisplayName = "Target URLs",
            HelpText = "The HTTPS URL using the certificate that this check will periodically verify. Multiple URLs " +
                       "can be checked; enter one per line.",
            InputType = SettingInputType.LongText)]
        public string TargetUrl { get; set; }

        [SeqAppSetting(
            DisplayName = "Interval (seconds)",
            IsOptional = true,
            HelpText = "The time between checks; the default is 3600 (once an hour).")]
        public int IntervalSeconds { get; set; } = 3600;

        [SeqAppSetting(DisplayName = "Minimum validity period (days)", IsOptional = true,
            HelpText = "The minimum amount of days a certificate should be valid; the default is 30")]
        public int ValidityDays { get; set; } = 30;

        public void Start(TextWriter inputWriter)
        {
            _httpClientWrapper = new HttpClientWrapper();
            var reporter = new CertificateCheckReporter(inputWriter);
            var targetUrls = TargetUrl.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var targetUrl in targetUrls)
            {
                var healthCheck = new CertificateValidityCheck(
                    App.Title,
                    targetUrl,
                    ValidityDays);

                _certificateCheckTasks.Add(new CertificateCheckTask(
                    healthCheck,
                    TimeSpan.FromSeconds(IntervalSeconds),
                    reporter,
                    _httpClientWrapper,
                    Log));
            }
        }

        public void Stop()
        {
            foreach (var task in _certificateCheckTasks)
            {
                task.Stop();
            }
        }

        public void Dispose()
        {
            foreach (var task in _certificateCheckTasks)
            {
                task.Dispose();
            }
        }
    }
}
