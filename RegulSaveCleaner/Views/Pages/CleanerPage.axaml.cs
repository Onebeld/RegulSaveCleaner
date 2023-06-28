using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using RegulSaveCleaner.Core;

namespace RegulSaveCleaner.Views.Pages;

public partial class CleanerPage : UserControl
{
    public CleanerPage() => InitializeComponent();

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        AssignCacheTypeLocalizationTemplates();
    }

    public void AssignCacheTypeLocalizationTemplates()
    {
        if (RegulSettings.Instance.Language == "zh")
        {
            WorldCachesTextBlock.Inlines = GetInlineCollection("WorldCacheChineseInline");
            IgaCacheTextBlock.Inlines = GetInlineCollection("IGACacheChineseInline");
            ThumbnailsTextBlock.Inlines = GetInlineCollection("ThumbnailsChineseInline");
            FeaturedItemsTextBlock.Inlines = GetInlineCollection("FeaturedItemsChineseInline");
            DcBackupTextBlock.Inlines = GetInlineCollection("DCBackupChineseInline");
            DccTextBlock.Inlines = GetInlineCollection("DccChineseInline");
            MissingDepsTextBlock.Inlines = GetInlineCollection("MissingDepsChineseInline");
            DownloadedSimsTextBlock.Inlines = GetInlineCollection("DownloadedSimsChineseInline");
        }
        else
        {
            WorldCachesTextBlock.Inlines = GetInlineCollection("WorldCacheInline");
            IgaCacheTextBlock.Inlines = GetInlineCollection("IGACacheInline");
            ThumbnailsTextBlock.Inlines = GetInlineCollection("ThumbnailsInline");
            FeaturedItemsTextBlock.Inlines = GetInlineCollection("FeaturedItemsInline");
            DcBackupTextBlock.Inlines = GetInlineCollection("DCBackupInline");
            DccTextBlock.Inlines = GetInlineCollection("DccInline");
            MissingDepsTextBlock.Inlines = GetInlineCollection("MissingDepsInline");
            DownloadedSimsTextBlock.Inlines = GetInlineCollection("DownloadedSimsInline");
        }
    }

    private InlineCollection GetInlineCollection(string key)
    {
        if (TryGetResource(key, null, out object? value) && value is InlineCollection inlineCollection)
            return inlineCollection;

        throw new KeyNotFoundException();
    }
}