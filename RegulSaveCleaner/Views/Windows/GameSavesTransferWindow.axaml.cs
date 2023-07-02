using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Styling;
using PleasantUI.Controls;
using RegulSaveCleaner.Core;
using RegulSaveCleaner.ViewModels.Windows;

namespace RegulSaveCleaner.Views.Windows;

public partial class GameSavesTransferWindow : ContentDialog
{
    public GameSavesTransferViewModel ViewModel { get; }
    
    public GameSavesTransferWindow()
    {
        InitializeComponent();

        DataContext = ViewModel = new GameSavesTransferViewModel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (RegulSettings.Instance.SaveFilesVerticalListDisplay is SaveFilesVerticalListDisplay.Block
            && Resources.TryGetResource("BlockItemSaveFilesDataTemplate", null, out object? value)
            && value is DataTemplate dataTemplate)
        {
            ListBox.DataTemplates.Clear();
            ListBox.DataTemplates.Add(dataTemplate);

            if (Application.Current is not null
                && Application.Current.TryFindResource("HorizontalListBoxItem", null, out object? otherValue) 
                && otherValue is ControlTheme controlTheme)
            {
                ListBox.ItemContainerTheme = controlTheme;

                ListBox.ItemsPanel = new FuncTemplate<Panel>(() => new WrapPanel());
            }
        }
        
        ViewModel.LoadOldSaves();
    }
}