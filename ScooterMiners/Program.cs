using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ScooterMiners;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSyncfusionBlazor();
SyncfusionLicenseProvider.RegisterLicense("NzE2MTgwQDMyMzAyZTMyMmUzMEx0Vm1xK3FLaWw5bkNsKzVvZ281VWltd2VuMW1RcjlyOG9ON2VVTjdpS0k9");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();