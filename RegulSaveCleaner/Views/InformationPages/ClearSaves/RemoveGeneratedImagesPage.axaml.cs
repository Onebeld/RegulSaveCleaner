using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RegulSaveCleaner.Views.InformationPages.ClearSaves;

public partial class RemoveGeneratedImagesPage : UserControl
{
    public RemoveGeneratedImagesPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        Random random = new();
        int percent = random.Next(0, 100);

        TheseCringeMachinesImage.IsVisible = percent is (< 35 and > 10) or 1;
        
        int percent1 = random.Next(0, 100);
        TheseKillingCringeMachinesImage.IsVisible = percent1 <= 10;
    }
}