
using BrowserInterop.Extensions;
using BrowserInterop.Geolocation;
using Geolocation;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ScooterMiners.Shared;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Syncfusion.Blazor.Buttons;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ScooterMiners.Pages
{
    public partial class Index
    {
        //[CascadingParameter(Name = "Notification")] Notification Notification { get; set; }

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected HttpClient Http { get; set; }

        private WindowNavigatorGeolocation geolocationWrapper;
        private Beacon? beacon;
        private GeolocationPosition? currentPosition;
        private IJSObjectReference module;
        private bool beaconEnabled, scanEnabled;
        private static CancellationTokenSource cancelTokenSource = new();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Index.razor.js");
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var window = await JSRuntime.Window();
            var navigator = await window.Navigator();
            geolocationWrapper = navigator.Geolocation;
        }

        public async Task<GeolocationPosition> GetGeolocation()
        {
            return (await geolocationWrapper.GetCurrentPosition(new PositionOptions()
            {
                EnableHighAccuracy = true,
                MaximumAgeTimeSpan = TimeSpan.FromSeconds(1),
                TimeoutTimeSpan = TimeSpan.FromMinutes(1)
            })).Location;
        }

        private async void ToggleBeacon(ChangeEventArgs<bool> args)
        {
            var a = await GetGeolocation();

            try
            {
                var uri = $"http://scooter.minstandart.online/Beacon/enable?args={args.Checked}";
                HttpRequestMessage request = new(HttpMethod.Get, new Uri(uri));
                request.SetBrowserRequestMode(BrowserRequestMode.NoCors);

                var response = await Http.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    beaconEnabled = args.Checked;
                    // Notification.Show(args ? "Мяак включен" : "Мяак выключен", args ? Notification.NotificationType.success : Notification.NotificationType.dark);
                }
            }
            catch (Exception error)
            {
                beaconEnabled = false;
                //  Notification.Show(error.Message, Notification.NotificationType.error);
                // Notification.Show("Ошибка соединения", Notification.NotificationType.error);
            }

            await InvokeAsync(StateHasChanged);
        }

        private async void ToggleScanner(bool args)
        {
            scanEnabled = args;
            //  Notification.Show(args ? "Поиск включен" : "Поиск выключен", args ? Notification.NotificationType.success : Notification.NotificationType.dark);

            if (args)
            {
                if (task == null || task.Status != TaskStatus.Running)
                {
                    cancelTokenSource = new();
                    await Task.Factory.StartNew(() => Update(), cancelTokenSource.Token);
                }
            }
            else
            {
                cancelTokenSource.Cancel();
            }

            await InvokeAsync(StateHasChanged);
        }

        private async void Update()
        {
            while (!cancelTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    beacon = await Http.GetFromJsonAsync<Beacon>("http://scooter.minstandart.online/Beacon");
                    currentPosition = await GetGeolocation();

                    Debug.WriteLine($"Lon: {currentPosition.Coords.Longitude} Lat: {currentPosition.Coords.Latitude}");
                }
                catch { }

                await Task.Delay(1000);
                await InvokeAsync(StateHasChanged);
            }
        }

        private readonly Task? task = new(() =>
        {
            while (!cancelTokenSource.Token.IsCancellationRequested)
            {
                Thread.Sleep(1000);
            }
        }, cancelTokenSource.Token);

        private double? GetDistance()
        {
            if (beacon == null || currentPosition == null) return null;

            return GeoCalculator.GetDistance(currentPosition.Coords.Latitude, currentPosition.Coords.Longitude, beacon.Latitude, beacon.Longitude, 2, DistanceUnit.Meters);
        }

        //private async void Beep()
        //{
        //    if (module != null)
        //    {
        //        await module.InvokeVoidAsync("beep", 200, 440, 100);
        //    }
        //}
    }
}