using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.Views.Pages.CleanerPages;

namespace RegulSaveCleaner.Views.Pages;

public partial class CleanerPage : UserControl
{
    public CleanerPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (RegulSettings.Instance.ListDisplay is ListDisplay.Vertical)
            Decorator.Child = new VerticalListCleanerPage();
        else
            Decorator.Child = new HorizontalListCleanerPage();
    }
}