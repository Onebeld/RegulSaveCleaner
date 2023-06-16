﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Win32;

namespace RegulSaveCleaner;

public class Program
{
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        appBuilder.UseSkia();
        
        appBuilder.UseWin32()
            .With(new AngleOptions
            {
                AllowedPlatformApis = new List<AngleOptions.PlatformApi>
                {
                    AngleOptions.PlatformApi.DirectX11
                }
            });

        appBuilder
            .With(new Win32PlatformOptions
            {
                AllowEglInitialization = true,
                OverlayPopups = true,
                UseWgl = false,
                UseWindowsUIComposition = true,
            })
            .With(new MacOSPlatformOptions
            {
                DisableDefaultApplicationMenuItems = true,
                ShowInDock = false
            })
            .With(new AvaloniaNativePlatformOptions
            {
                UseGpu = true,
            });

        return appBuilder.LogToTrace();
    }
}