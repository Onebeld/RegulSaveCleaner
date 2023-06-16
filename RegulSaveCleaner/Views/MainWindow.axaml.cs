using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using PleasantUI.Core.Enums;

namespace RegulSaveCleaner.Views;

public partial class MainWindow : PleasantWindow
{
    public MainWindow() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

#if !NET461
        if (OperatingSystem.IsMacOS())
        {
            EnableTitleBarMargin = true;
            TitleBarType = PleasantTitleBarType.Classic;
        }
#endif
    }
}