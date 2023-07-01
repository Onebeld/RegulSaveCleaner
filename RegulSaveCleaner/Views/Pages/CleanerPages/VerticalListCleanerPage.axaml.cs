using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace RegulSaveCleaner.Views.Pages.CleanerPages;

public partial class VerticalListCleanerPage : UserControl
{
    public VerticalListCleanerPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DownButton.Click += (_, _) => Carousel.Next();
        UpButton.Click += (_, _) => Carousel.Previous();
    }
}