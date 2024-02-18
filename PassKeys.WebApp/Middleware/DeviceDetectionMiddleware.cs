using System.Text;
using DeviceDetectorNET;
using Microsoft.Extensions.Primitives;
using PassKeys.WebApp.Utils;

namespace PassKeys.WebApp.Middleware;

public class DeviceDetectionMiddleware(RequestDelegate next)
{
    const string UAKey = "User-Agent";
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(UAKey, out var uaHeader))
        {
            var deviceDetector = new DeviceDetector(uaHeader);
            if (deviceDetector.IsBot())
            {
                context.Items[Constants.Device.PlatformInfoKey] = "Unknown";
            }

            deviceDetector.Parse();

            var platformInfoBuilder = new StringBuilder();
            platformInfoBuilder.Append(deviceDetector.GetDeviceName());
            platformInfoBuilder.Append(' ');
            platformInfoBuilder.Append(deviceDetector.GetBrandName());
            platformInfoBuilder.Append(' ');
            var osInfo = deviceDetector.GetOs().Match;
            if (osInfo != null)
            {
                platformInfoBuilder.Append(osInfo.Name);
                platformInfoBuilder.Append(' ');
                platformInfoBuilder.Append(osInfo.Version);
                platformInfoBuilder.Append(' ');
            }

            var clientInfo = deviceDetector.GetClient().Match;
            if (clientInfo != null)
            {
                platformInfoBuilder.Append(clientInfo.Name);
                platformInfoBuilder.Append(' ');
                platformInfoBuilder.Append(clientInfo.Version);
                platformInfoBuilder.Append(' ');
            }

            context.Items[Constants.Device.PlatformInfoKey] = platformInfoBuilder.ToString();
        }

        await next(context);
    }
}