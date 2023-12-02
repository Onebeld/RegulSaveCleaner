using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using RegulSaveCleaner.Structures;
using RegulSaveCleaner.ViewModels.Windows;

namespace RegulSaveCleaner.Views.Windows;

public partial class ProhibitedListWindow : ContentDialog
{
    public ProhibitedListViewModel ViewModel { get; }
    
    public ProhibitedListWindow() => InitializeComponent();

    public ProhibitedListWindow(GameSave gameSave) : this()
    {
        DataContext = ViewModel = new ProhibitedListViewModel(gameSave);
        TemplateApplied += OnTemplateApplied;
    }

    private void OnTemplateApplied(object? sender, TemplateAppliedEventArgs e) => ViewModel.LoadImages();
}