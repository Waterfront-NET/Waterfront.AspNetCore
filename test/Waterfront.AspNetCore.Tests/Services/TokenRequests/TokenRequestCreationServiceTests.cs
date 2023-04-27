using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Waterfront.AspNetCore.Services.TokenRequests;

namespace Waterfront.AspNetCore.Tests.Services.TokenRequests;

[TestClass]
public class TokenRequestCreationServiceTests
{
    /*static Mock<ILogger<TokenRequestCreationService>> MockLogger;

    [ClassInitialize]
    public static void Initialize(TestContext ctx)
    {
        MockLogger = new Mock<ILogger<TokenRequestCreationService>>();
    }*/

    [TestMethod]
    public async Task TestNoService()
    {
        using var host = await new HostBuilder().ConfigureWebHost(
                                                    builder =>
                                                    {
                                                        builder.UseTestServer()
                                                               .ConfigureServices(
                                                                   services =>
                                                                   {
                                                                       services.AddLogging();
                                                                       services
                                                                           .AddSingleton<TokenRequestCreationService>();
                                                                   }
                                                               )
                                                               .Configure(
                                                                   app =>
                                                                   {
                                                                       app.Map(
                                                                           "/test",
                                                                           appBuilder =>
                                                                           {
                                                                               appBuilder.Use(
                                                                                   async (ctx, next) =>
                                                                                   {
                                                                                       var srv = ctx.RequestServices
                                                                                           .GetRequiredService<
                                                                                               TokenRequestCreationService>();

                                                                                       var logger = ctx.RequestServices
                                                                                           .GetRequiredService<
                                                                                               ILogger<
                                                                                                   IApplicationBuilder>>();

                                                                                       var tokenRequest =
                                                                                           srv.CreateRequest(ctx);

                                                                                       logger.LogInformation(
                                                                                           JsonConvert.SerializeObject(
                                                                                               tokenRequest,
                                                                                               Formatting.Indented
                                                                                           )
                                                                                       );

                                                                                       await next(ctx);
                                                                                   }
                                                                               );
                                                                           }
                                                                       );
                                                                   }
                                                               );
                                                    }
                                                )
                                                .StartAsync();

        var response = await host.GetTestClient().GetAsync("/test");

        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
    }
}
