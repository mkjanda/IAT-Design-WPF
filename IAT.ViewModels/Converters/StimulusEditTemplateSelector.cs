using IAT.ViewModels.Controls;
using System.Windows;
using System.Windows.Controls;

namespace IAT.ViewModels.Converters;

/// <summary>
/// A DataTemplateSelector that selects the appropriate DataTemplate for editing stimuli based on the type of the ViewModel.
/// </summary>
public class StimulusEditTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Gets or sets the DataTemplate for editing text stimuli.
    /// </summary>
    public DataTemplate? TextEditTemplate { get; set; }

    /// <summary>
    /// Gets or sets the DataTemplate for editing image stimuli.
    /// </summary>
    public DataTemplate? ImageEditTemplate { get; set; }


    /// <summary>
    /// Selects the appropriate DataTemplate based on the type of the item.
    /// </summary>
    /// <param name="item">The item for which to select the template.</param>
    /// <param name="container">The container in which the template will be applied.</param>
    /// <returns>The selected DataTemplate.</returns>
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        return item switch
        {
            ImageStimulusEditViewModel => ImageEditTemplate,
            StimulusEditViewModel => TextEditTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}