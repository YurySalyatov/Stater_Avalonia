using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Stater.Models.Editors;

namespace Stater.Converters;

public class EditorTypeToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null || value == null)
            return false;

        var editorType = (EditorTypeEnum)value;
        if (Enum.TryParse(parameter.ToString(), out EditorTypeEnum targetEditorType))
        {
            return editorType == targetEditorType;
        }

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}