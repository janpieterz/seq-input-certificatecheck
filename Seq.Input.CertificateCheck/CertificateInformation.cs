using System;
using System.Collections.Generic;

namespace Seq.Input.CertificateCheck
{
    public class CertificateInformation
    {
        public string Subject { get; set; }
        public string Issuer { get; set; }
        public DateTime? LastExpiration { get; set; }
        public string Thumbprint { get; set; }
        public string SerialNumber { get; set; }
        public IEnumerable<string> SubjectAlternativeNames { get; set; }
    }
}