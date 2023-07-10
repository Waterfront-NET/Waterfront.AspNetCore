using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Waterfront.AspNetCore.Services.TokenRequests;
using Waterfront.Common.Tokens.Requests;

namespace Waterfront.AspNetCore.Tests.Services.TokenRequests;

[TestClass]
public class TokenRequestCreationServiceTests
{
#pragma warning disable CS8618
    private static IHost host;
#pragma warning restore CS8618

    [TestInitialize]
    public void TestInitialize()
    {
        /*Create test host*/
        host = new HostBuilder()
            .ConfigureWebHost(
                webhostBuilder =>
                    webhostBuilder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services.AddLogging().AddSingleton<TokenRequestService>();
                        })
                        .Configure(applicationBuilder =>
                        {
                            applicationBuilder.Map(
                                "/request-creation-test",
                                app =>
                                {
                                    app.Use(
                                        async (context, next) =>
                                        {
                                            try
                                            {
                                                TokenRequest request = context.RequestServices
                                                    .GetRequiredService<TokenRequestService>()
                                                    .CreateRequest(context);
                                            }
                                            catch (InvalidOperationException ioe)
                                            {
                                                await Results
                                                    .BadRequest(ioe.Message)
                                                    .ExecuteAsync(context);
                                                return;
                                            }

                                            await next();
                                        }
                                    );
                                }
                            );
                        })
            )
            .Start();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        host.StopAsync().GetAwaiter().GetResult();
        host.Dispose();
    }

    [TestMethod]
    public async Task TestNoService()
    {
        var response = await host.GetTestClient().GetAsync("/request-creation-test");

        response.Should().HaveStatusCode(HttpStatusCode.BadRequest);
    }
}
