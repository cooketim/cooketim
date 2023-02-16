using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using DataLib;

namespace TimsTool.Tree.Binding
{
    public class EnumStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) { return string.Empty; }
                var enumName = Enum.GetName(value.GetType(), value);

                if(string.IsNullOrEmpty(enumName)) { return string.Empty; }

                //see if we have a more friendly description
                Enum enumVal = (Enum)Enum.Parse(value.GetType(), enumName);
                var enumDescription = enumVal.GetDescription();

                return string.IsNullOrEmpty(enumDescription) ? enumName : enumDescription;
            }
            catch
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : System.Windows.Data.Binding.DoNothing;
        }

        #endregion
    }
}
