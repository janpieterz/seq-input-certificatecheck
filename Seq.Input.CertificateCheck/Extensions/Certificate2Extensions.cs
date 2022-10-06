using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Seq.Input.CertificateCheck.Extensions
{
    public static class Certificate2Extensions
    {
        public static IEnumerable<string> ParseSubjectAlternativeNames(this X509Certificate2 cert)
        {
            Regex sanRex = new Regex(@"^DNS Name=(.*)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var sanList = from X509Extension ext in cert.Extensions
                where "2.5.29.17".Equals(ext.Oid.Value, StringComparison.Ordinal) // OID for Subject Alternative Name
                let text = ext.Format(true)
                from line in text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                let match = sanRex.Match(line)
                where match.Success && match.Groups.Count > 0 && !string.IsNullOrEmpty(match.Groups[1].Value)
                select match.Groups[1].Value;

            return sanList;
        }
    }
}