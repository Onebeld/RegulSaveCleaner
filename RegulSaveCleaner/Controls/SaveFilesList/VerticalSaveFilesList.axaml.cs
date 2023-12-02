using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using RegulSaveCleaner.Core;

namespace RegulSaveCleaner.Controls.SaveFilesList;

public partial class VerticalSaveFilesList : UserControl
{
    public VerticalSaveFilesList()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (RegulSettings.Instance.SaveFilesVerticalListDisplay is not SaveFilesVerticalListDisplay.Block
            || !Resources.TryGetResource("BlockItemSaveFilesDataTemplate", null, out object? value)
            || value is not DataTemplate dataTemplate)
            return;

        ListBox.DataTemplates.Clear();
        ListBox.DataTemplates.Add(dataTemplate);

        if (Application.Current is null
            || !Application.Current.TryFindResource("HorizontalListBoxItem", null, out object? otherValue)
            || otherValue is not ControlTheme controlTheme)
            return;

        ListBox.ItemContainerTheme = controlTheme;

        ListBox.ItemsPanel = new FuncTemplate<Panel>(() => new WrapPanel());
    }
}