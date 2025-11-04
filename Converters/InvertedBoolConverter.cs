using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace SuporteTiApp.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        // ✅ CORRIGIDO: Adicionar ? para nullable
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            
            return false;
        }

        // ✅ CORRIGIDO: Adicionar ? para nullable  
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
                
            return false;
        }
    }
}