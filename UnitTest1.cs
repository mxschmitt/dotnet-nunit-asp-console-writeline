using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NUnit.Framework.Internal;

namespace dotnet_nunit_asp_console_writeline;

public class Tests
{
    [Test]
    public async Task TestLoggingInRequestHandler()
    {
        // Needed for Debug/Trace calls.
        Trace.Listeners.Add(new ConsoleTraceListener());
        var builder = new WebHostBuilder()
            .Configure(app =>
            {
                var currentExecutionContext = TestExecutionContext.CurrentContext;
                app.Run(async context =>
                {
                    {
                        // This hack allows us to have Console.WriteLine etc. appear in the test output.
                        var currentContext = typeof(TestExecutionContext).GetField("AsyncLocalCurrentContext", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as AsyncLocal<TestExecutionContext>;
                        currentContext.Value = currentExecutionContext;
                    }
                    Debug.WriteLine("1. This is Debug.WriteLine");
                    Trace.WriteLine("2. This is Trace.WriteLine");
                    Console.WriteLine("3. This is Console.Writeline");
                    Console.Error.WriteLine("4. This is Console.Error.Writeline");
                    TestContext.WriteLine("5. This is TestContext.WriteLine");
                    TestContext.Out.WriteLine("6. This is TestContext.Out.WriteLine");
                    TestContext.Progress.WriteLine("7. This is TestContext.Progress.WriteLine");
                    TestContext.Error.WriteLine("8. This is TestContext.Error.WriteLine");
                    await context.Response.WriteAsync("Hello, world!");
                });
            })
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Loopback, 1234);
            })
            .Build();
        await builder.StartAsync();

        var httpclient = new HttpClient();
        var response = await httpclient.GetAsync("http://localhost:1234");
        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Hello, world!"));
        await builder.StopAsync();
    }
}