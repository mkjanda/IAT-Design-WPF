using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.Views.Converters;

public class StimulusEditTemplateSelector : DataTemplateSelector
{
    public DataTemplate? TextEditTemplate { get; set; }
    public DataTemplate? ImageEditTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            StimulusEditViewModel => TextEditTemplate,
            ImageStimulusEditViewModel => ImageEditTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}