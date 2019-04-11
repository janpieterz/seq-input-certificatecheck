using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Seq.Apps;
using Seq.Input.CertificateCheck;
using Serilog;

namespace Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        public async Task Run()
        {
            using (StreamWriter writer = new StreamWriter(new MemoryStream()))
            {
                CertificateCheckInput runner = new CertificateCheckInput
                {
                    TargetUrl = $"https://api.arke.io/up{Environment.NewLine}https://app.arke.io/up.json",
                    IntervalSeconds = 5
                };
                var testHost = new TestHost {App = new App("1", "test", new Dictionary<string, string>(), "")};

                runner.Attach(testHost);
                runner.Start(writer);
                Console.ReadLine();
            }
        }
    }

    public class TestHost : IAppHost
    {
        public App App { get; set; }
        public Host Host { get; set; }
        public ILogger Logger { get; set; }
        public string StoragePath { get; set; }
    }
}
