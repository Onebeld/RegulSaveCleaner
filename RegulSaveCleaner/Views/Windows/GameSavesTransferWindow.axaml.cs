using Avalonia.Controls.Primitives;
using PleasantUI.Controls;
using RegulSaveCleaner.ViewModels.Windows;

namespace RegulSaveCleaner.Views.Windows;

public partial class GameSavesTransferWindow : ContentDialog
{
    public GameSavesTransferViewModel ViewModel { get; private set; }
    
    public GameSavesTransferWindow()
    {
        InitializeComponent();

        DataContext = ViewModel = new GameSavesTransferViewModel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        ViewModel.LoadOldSaves();
    }
}