﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Seq.Input.CertificateCheck
{
    public class CertificateCheckResult
    {
        [JsonProperty("@t")]
        public DateTime UtcTimestamp { get; }

        [JsonProperty("@mt")]
        public string MessageTemplate { get; } =
            "Certificate check {TargetUrl} {Outcome}, expires in {ExpirationDays} days";

        [JsonProperty("@l", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Level { get; }
        public DateTime? Expiration { get; set; }
        public int ExpirationDays => Expiration.HasValue ? Convert.ToInt32(Math.Floor((Expiration.Value - DateTime.UtcNow).TotalDays)) : -1;
        public string Issuer { get; }
        public string Subject { get; }
        public string Thumbprint { get; }
        public string SerialNumber { get; set; }
        public IEnumerable<string> SubjectAlternativeNames { get; set; }
        public string CertificateCheckTitle { get; }
        public string TargetUrl { get; }
        public string Outcome { get; }
        
        public CertificateCheckResult(
            DateTime utcTimestamp,
            string certificateCheckTitle,
            string targetUrl,
            string outcome,
            string level,
            DateTime? expiration,
            string issuer,
            string subject,
            string thumbprint,
            string serialNumber,
            IEnumerable<string> subjectAlternativeNames)
        {
            if (utcTimestamp.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The timestamp must be UTC.", nameof(utcTimestamp));

            UtcTimestamp = utcTimestamp;

            CertificateCheckTitle= certificateCheckTitle?? throw new ArgumentNullException(nameof(certificateCheckTitle));
            TargetUrl = targetUrl ?? throw new ArgumentNullException(nameof(targetUrl));
            Outcome = outcome ?? throw new ArgumentNullException(nameof(outcome));
            Level = level;
            Expiration = expiration;
            Issuer = issuer;
            Subject = subject;
            Thumbprint = thumbprint;
            SerialNumber = serialNumber;
            SubjectAlternativeNames = subjectAlternativeNames;
        }
    }
}