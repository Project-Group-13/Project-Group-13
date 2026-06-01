using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(HeatProductionOptimizationTests.TestAppBuilder))]

namespace HeatProductionOptimizationTests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Heat_Production_Optimization.App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}
