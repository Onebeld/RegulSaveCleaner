﻿using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Win32;

namespace RegulSaveCleaner;

public class Program
{
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        if (!Directory.Exists(pathToLog)) Directory.CreateDirectory(pathToLog);

        if (e.ExceptionObject is Exception ex)
        {
            string filename = $"{AppDomain.CurrentDomain.FriendlyName}_{DateTime.Now:dd.MM.yyy}.log";

            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine($"[{DateTime.Now:dd.MM.yyy HH:mm:ss.fff}] | Fatal | {ex}\n");

            OperatingSystem os = Environment.OSVersion;
            stringBuilder.AppendLine("Operating System information:");
            stringBuilder.AppendLine("OS name: " + os.VersionString);
            stringBuilder.AppendLine("OS architecture: " + RuntimeInformation.OSArchitecture + "\n");
            
            CultureInfo cultureInfo = CultureInfo.CurrentUICulture;
            stringBuilder.AppendLine("Default language information:");
            stringBuilder.AppendLine("Name: " + cultureInfo.Name);
            stringBuilder.AppendLine("English name: " + cultureInfo.EnglishName);
            stringBuilder.AppendLine("2-letter ISO name: " + cultureInfo.TwoLetterISOLanguageName + "\n");
            
            stringBuilder.AppendLine("Other information:");
            stringBuilder.AppendLine(".NET version: " + RuntimeInformation.FrameworkDescription);
            stringBuilder.AppendLine("Process architecture: " + RuntimeInformation.ProcessArchitecture + "\r\n");

            File.AppendAllText(Path.Combine(pathToLog, filename), stringBuilder.ToString(), Encoding.UTF8);

            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(pathToLog, filename),
                UseShellExecute = true
            });
        }
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        AppBuilder appBuilder = AppBuilder.Configure<App>();
        appBuilder.UseSkia();

#if Windows
        appBuilder.UseWin32()
            .With(new AngleOptions
            {
                AllowedPlatformApis = new List<AngleOptions.PlatformApi>
                {
                    AngleOptions.PlatformApi.DirectX11
                }
            });
#elif Linux
        appBuilder.UseX11();
#elif OSX
        appBuilder.UseAvaloniaNative();
#else
        appBuilder.UsePlatformDetect();
#endif
        
        appBuilder
#if Windows
            .With(new Win32PlatformOptions
            {
                OverlayPopups = true,
            });
#endif
#if OSX
            .With(new MacOSPlatformOptions
            {
                DisableDefaultApplicationMenuItems = true,
                ShowInDock = false,
                DisableNativeMenus = true
            });
#endif
#if Linux
            .With(new X11PlatformOptions()
            {
                OverlayPopups = true
            });
#endif

        return appBuilder.LogToTrace();
    }
}