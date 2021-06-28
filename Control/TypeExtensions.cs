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

            return type
                .Name
                .ToSnakeCase()
                ;

        }

        public static string GetFormName(this PropertyInfo property)
        {

            return property.PropertyType == typeof(string)
                ? property.Name
                : property.PropertyType.Name.ToSnakeCase()
                ;

        }


    }
}
