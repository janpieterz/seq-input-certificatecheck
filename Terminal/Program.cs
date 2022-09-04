using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Seq.Apps;
using Seq.Input.CertificateCheck;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact.Reader;

namespace Terminal
{
    static class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "https://api.arke.io/up", "https://app.arke.io/up.json" };
            }
            Logger generalLog = new Serilog.LoggerConfiguration().WriteTo.Console(outputTemplate: "[MAIN][{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}").CreateLogger();
            Logger selfLog = new Serilog.LoggerConfiguration().WriteTo.Console(outputTemplate: "[\x1b[43;34mSELF\x1b[0m][{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Properties}{NewLine}{Exception}").CreateLogger();
            TestHost testHost = new TestHost { App = new App("1", "test", new Dictionary<string, string>() { }, ""), Logger = selfLog };

            AnonymousPipeServerStream hostPipe = new AnonymousPipeServerStream(PipeDirection.In);
            CancellationTokenSource cts = new CancellationTokenSource();
            StreamReader streamLogReader = new StreamReader(hostPipe);
            Task hostTask = RunSimulatedHost(testHost, args, hostPipe.GetClientHandleAsString(), cts.Token);

            Task<string> exitTask = Task.Run<string>(Console.In.ReadLine);

            while (!cts.IsCancellationRequested)
            {
                Task<string> newEventTask = streamLogReader.ReadLineAsync();
                Task finished = await Task.WhenAny(newEventTask, exitTask);
                if (finished == exitTask)
                {
                    cts.Cancel();
                }
                else // newEventTask
                {
                    try
                    {
                        string sEvent = await newEventTask;
                        JObject jEvent = JObject.Parse(sEvent);
                        LogEvent lEvent = LogEventReader.ReadFromJObject(jEvent);
                        generalLog.Write(lEvent);
                    }
                    catch(Exception ex)
                    {
                        string sEvent = "";
                        try
                        {
                            sEvent = await newEventTask;
                        }
                        catch { }
                        selfLog.ForContext("Event", sEvent).Fatal(ex, "HOST: Could not read event.");
                    }
                }
            }
            await Task.WhenAll(hostTask);
        }

        static async Task RunSimulatedHost(TestHost testHost, string[] targetUrls, string pipeHandle, CancellationToken cancellationToken)
        {
            AnonymousPipeClientStream clientPipe = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandle);
            using (StreamWriter writer = new StreamWriter(clientPipe))
            {
                CertificateCheckInput runner = new CertificateCheckInput
                {
                    TargetUrl = string.Join(Environment.NewLine, targetUrls),
                    IntervalSeconds = 5
                };

                runner.Attach(testHost);
                runner.Start(writer);
                try
                {
                    await Task.Delay(-1, cancellationToken);
                }
                catch (TaskCanceledException) { }
                runner.Stop();
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
