﻿using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using PleasantUI.Controls;

namespace RegulSaveCleaner.Views.Windows;

public partial class ImageViewerWindow : ContentDialog
{
    public ImageViewerWindow() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        CloseButton.Click += CloseButtonOnClick;
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}