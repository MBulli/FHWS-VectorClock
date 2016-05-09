using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorClock.Commander.Helper;

namespace VectorClock.Commander.ViewModel.Converters
{
    public class BooleanToBrushConverter : ConverterBase
    {
        public System.Windows.Media.Brush FalseBrush { get; set; }
        public System.Windows.Media.Brush TrueBrush { get; set; }

        public BooleanToBrushConverter()
        {
            FalseBrush = System.Windows.Media.Brushes.Red;
            TrueBrush = System.Windows.Media.Brushes.Green;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(System.Windows.Media.Brush))
                return null;

            bool? b = value as bool?;
            if (b == null)
                return null;

            return b.Value ? TrueBrush : FalseBrush;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
