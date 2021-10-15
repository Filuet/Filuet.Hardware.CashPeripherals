using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Filuet.Hardware.CashAcceptors.Common.Attributes
{
    public class CodeEnumConverter<TEnum> : TypeConverter
        where TEnum : new() // TEnum : Enum is the 7.3 feature
    {
        private static Dictionary<string, FieldInfo> codeMap;

        static CodeEnumConverter()
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public;

            codeMap = typeof(TEnum).GetFields(bindingFlags).ToDictionary(
                field =>
                {
                    CodeAttribute attr = (CodeAttribute)Attribute.GetCustomAttribute(field, typeof(CodeAttribute));
                    return attr.DisplayCode;
                });
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string valueStr = Convert.ToString(value);

            if (codeMap.ContainsKey(valueStr))
            {
                return codeMap[valueStr].GetValue(new TEnum());
            }

            throw new InvalidCastException($"Cannot convert '{valueStr}' to {typeof(TEnum)}");
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (!value.GetType().Equals(typeof(TEnum)))
            {
                throw new InvalidCastException($"Can only convert an instance of {typeof(TEnum)}.");
            }

            string name = Convert.ToString(value);

            object[] attrs =
                value.GetType().GetField(name).GetCustomAttributes(typeof(CodeAttribute), false);

            if (attrs.Length == 0)
            {
                throw new InvalidCastException($"Cannot convert '{value}' to {destinationType}");
            }

            return ((CodeAttribute)attrs[0]).DisplayCode;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }
    }
}
