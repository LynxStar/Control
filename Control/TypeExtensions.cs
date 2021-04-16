using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Control
{
    public static class TypeExtensions
    {

        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public static string GetFormName(this Type type)
        {

            var formAttr = type
                .GetCustomAttributes(typeof(FormAttribute), true)
                .FirstOrDefault()
                as FormAttribute
                ;

            return formAttr is null
                ? type.Name
                : formAttr.Name
                ;

        }

        public static string GetFormName(this PropertyInfo property)
        {

            var formAttr = property
                .GetCustomAttributes(typeof(FormAttribute), true)
                .FirstOrDefault()
                as FormAttribute
                ;

            return formAttr is null
                ? property.Name
                : formAttr.Name
                ;

        }


    }
}
