using System;
using System.IO;
using Newtonsoft.Json;

namespace Seq.Input.CertificateCheck
{
    class CertificateCheckReporter
    {
        readonly TextWriter _output;
        readonly JsonSerializer _serializer = JsonSerializer.Create();
        readonly object _sync = new object();

        public CertificateCheckReporter(TextWriter output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        public void Report(CertificateCheckResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            lock (_sync)
            {
                _serializer.Serialize(_output, result);
                _output.WriteLine();
                _output.Flush();
            }
        }
    }
}