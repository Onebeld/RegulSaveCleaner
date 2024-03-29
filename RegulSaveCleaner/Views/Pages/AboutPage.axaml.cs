﻿using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI;

namespace RegulSaveCleaner.Views.Pages;

public partial class AboutPage : UserControl
{
    public AboutPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        Version appVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        AppVersion.Text = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";

        Version pleasantUIVersion = typeof(PleasantTheme).Assembly.GetName().Version!;
        PleasantUIVersion.Text = $"{pleasantUIVersion.Major}.{pleasantUIVersion.Minor}.{pleasantUIVersion.Build}";

#if NET6_0_OR_GREATER
        ImageSharpVersion.Text = "3.0.2";
#else
        ImageSharpVersion.Text = $"2.1.4";
#endif

        RunDotNet.Text = $"{RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture}";
        RunAuthor.Text = $"©2020-{DateTime.Now.Year} Dmitry Zhutkov (Onebeld)";
        
        GitHubButton.Click += GitHubButtonOnClick;
        PatreonButton.Click += PatreonButtonOnClick;
        
        MailButton.Click += MailButtonOnClick;
        DiscordButton.Click += DiscordButtonOnClick;

        if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName != "ru")
            SocialNetwork.IsVisible = false;
        else
        {
            SocialNetworkButton.Click += SocialNetworkButtonOnClick;
            SocialNetworkButton.Click += MenuButtonsOnClick;
        }

        TelegramButton.Click += TelegramButtonOnClick;

        MailButton.Click += MenuButtonsOnClick;
        DiscordButton.Click += MenuButtonsOnClick;
        TelegramButton.Click += MenuButtonsOnClick;
    }

    private void TelegramButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://t.me/onebeld",
            UseShellExecute = true
        });
    }

    private void SocialNetworkButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://vk.com/onebeld",
            UseShellExecute = true
        });
    }

    private void DiscordButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://discordapp.com/users/546992251562098690",
            UseShellExecute = true
        });
    }

    private void MailButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "mailto:onebeld@gmail.com",
            UseShellExecute = true,
        });
    }

    private void MenuButtonsOnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
            button.Command?.Execute(button.CommandParameter);

        ContactAuthorButton.Flyout?.Hide();
    }

    private void PatreonButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/onebeld",
            UseShellExecute = true
        });
    }

    private void GitHubButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Onebeld/RegulSaveCleaner",
            UseShellExecute = true
        });
    }
}