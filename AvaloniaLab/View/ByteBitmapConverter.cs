using System;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Platform;

namespace View
{
    public class ByteBitmapConverter : IValueConverter
    {
        public static BitmapConverter Instance = new BitmapConverter();
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            // if (value == null)
            //     return null;

            // return new Bitmap((string)value);

            Bitmap bmp;
            using (var ms = new MemoryStream((byte[])value))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}