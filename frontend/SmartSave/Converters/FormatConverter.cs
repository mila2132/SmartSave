using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSave.Converters
{
	public class HourFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string hourStr && int.TryParse(hourStr, out int hour))
			{
				return $"{hour:00}:00";
			}

			return value; // Devuelve el valor original si no se puede convertir
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class PriceFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double price)
			{
				return $"{price:0.000} €kWh";
			}

			return value; // Devuelve el valor original si no se puede convertir
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}	


}
