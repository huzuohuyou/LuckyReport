using AntDesign.ProLayout;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Append.Blazor.Printing;

namespace LuckyReport;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddAntDesign();
        builder.Services.Configure<ProSettings>(builder.Configuration.GetSection("ProSettings"));
        builder.Services.AddScoped<IPrintingService, PrintingService>();
        //builder.Services.HttpClientFactoryServiceCollectionExtensions.AddHttpClient();
        await builder.Build().RunAsync();
    }
}