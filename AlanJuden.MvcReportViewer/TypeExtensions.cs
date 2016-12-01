using System;
using System.Collections.Generic;
using System.Linq;

namespace AlanJuden.MvcReportViewer
{
    public static class TypeExtensions
    {
        public static string[] COMMON_DELIMETERS = { ",", "\r\n", "^", ";", "+", "\r", "\n", "\t" };

        public static string ToSafeString(this object value)
        {
            return ToSafeString(value, string.Empty);
        }

        public static string ToSafeString(object value, string defaultValue)
        {
            try
            {
                return value?.ToString() ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Takes ANY value whatsoever and makes it a usable string that has no leading or trailing whitespace.
        /// </summary>
        public static string SafeTrim(this object value)
        {
            return ToSafeString(value, string.Empty).Trim();
        }

        public static bool ToBoolean(this object value)
        {
            return ToBoolean(value, default(bool));
        }

        public static bool ToBoolean(object value, bool defaultValue)
        {
            try
            {
                if (value == null)
                {
                    return defaultValue;
                }
                else
                {
                    string val = value.ToString().ToUpper();
                    //VB6 code set the True value to -1, check for this
                    switch (val)
                    {
                        case "1":
                        case "-1":
                        case "T":
                        case "TRUE":
                        case "Y":
                        case "CHECKED":
                        case "ON":
                            return true;
                        case "0":
                        case "F":
                        case "FALSE":
                        case "N":
                        case "UNCHECKED":
                        case "OFF":
                            return false;
                        default:
                            return Convert.ToBoolean(value);
                    }
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int ToInt32(this object value)
        {
            return ToInt32(value, default(int));
        }

        public static int ToInt32(object value, int defaultValue)
        {
            try
            {
                return value == null ? defaultValue : Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Turns a string into a List<string>
        /// </summary>
        /// <param name="data">Data to be turned into a list</param>
        /// <returns></returns>
        public static List<string> ToStringList(this string data)
        {
            return ToStringList(data, COMMON_DELIMETERS);
        }

        /// <summary>
        /// Turns a string into a List<string>
        /// </summary>
        /// <param name="data">Data to be turned into a list</param>
        /// <param name="separators">List of separators you want to split your string on</param>
        /// <returns></returns>
        public static List<string> ToStringList(this string data, string[] separators)
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                var array = data.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                foreach (var asString in array.Select(token => token.SafeTrim()).Where(asString => !list.Contains(asString)))
                {
                    list.Add(asString);
                }
            }
            return list;
        }

        public static string HtmlEncode(this string text)
        {
            return System.Web.HttpUtility.HtmlEncode(text.ToSafeString());
        }

        public static string UrlEncode(this string text)
        {
            return System.Web.HttpUtility.UrlEncode(text.ToSafeString());
        }

        public static string HtmlDecode(this string text)
        {
            return System.Web.HttpUtility.HtmlDecode(text.ToSafeString());
        }

        public static string UrlDecode(this string text)
        {
            return System.Web.HttpUtility.UrlDecode(text.ToSafeString());
        }

        public static bool HasValue(this string Value)
        {
            var emptyList = new List<string>() { "", "N/A", "NA", "TBD" };

            return !string.IsNullOrEmpty(Value.SafeTrim())
                && !emptyList.Contains(Value.SafeTrim().ToUpper());
        }

        public static string GetName(this Enum value)
        {
            var info = value.GetType().GetField(value.ToString());

            try
            {
                var attribs = (EnumNameAttribute[])info.GetCustomAttributes(typeof(EnumNameAttribute), false);

                return attribs.Length > 0 ? attribs[0].Name : value.ToString();
            }
            catch
            {
                return value.ToString();
            }
        }

        public static T ToEnum<T>(this string name)
        //where T : Enum
        {
            return (T)Enum.Parse(typeof(T), name);
        }


        public static T NameToEnum<T>(this string name)
        // where T : Enum
        {
            return (
                from val in Enum.GetNames(typeof(T)).AsEnumerable()
                let enumItem = Enum.Parse(typeof(T), val)
                where GetName((Enum)enumItem).Equals(name, StringComparison.InvariantCultureIgnoreCase)
                select (T)enumItem
                ).Single();
        }
    }
}
