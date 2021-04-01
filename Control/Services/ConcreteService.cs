using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{
    
    public class ConcreteService
    {


        public T MapTo<T>(SyntaxNode node) where T : new()
        {

            var properties = typeof(T).GetProperties();

            T result = new T();

            foreach(var property in properties)
            {

                object value = null;

                var cgrAttribute = property
                    .GetCustomAttributes(typeof(CGRAttribute), true)
                    .OfType<CGRAttribute>()
                    .SingleOrDefault()
                    ;

                if(cgrAttribute is not null)
                {

                    var cgrNode = node
                        .SyntaxNodes
                        .Where(x => x.Rule.Name.Contains("CGR"))
                        .ToList()
                        [cgrAttribute.Position]
                        ;

                    //Collection
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {

                        var method = typeof(ConcreteService).GetMethod("GetCGRValue");

                        var listInnerType = property.PropertyType.GenericTypeArguments.First();

                        var genericMethod = method.MakeGenericMethod(listInnerType);
                        value = genericMethod.Invoke(this, new object[] { cgrNode, cgrAttribute });

                    }
                    //Single
                    else
                    {

                        if (cgrAttribute.MapInner)
                        {
                            cgrNode = ApplyCGRProjection(cgrNode, cgrAttribute);
                        }

                        var method = typeof(ConcreteService).GetMethod("MapTo");
                        var genericMethod = method.MakeGenericMethod(property.PropertyType);
                        value = genericMethod.Invoke(this, new object[] { cgrNode });

                    }

                }

                else if (property.PropertyType == typeof(string))
                {
                    value = GetTokenValue(node, property.Name);
                }

                else
                {
                    var method = typeof(ConcreteService).GetMethod("GetFieldValue");
                    var genericMethod = method.MakeGenericMethod(property.PropertyType);
                    value = genericMethod.Invoke(this, new object[] { node, property.Name });
                }

                property.SetValue(result, value);

            }

            return result;

        }

        public SyntaxNode ApplyCGRProjection(SyntaxNode cgrNode, CGRAttribute cgrAttribute)
        {

            if(!cgrAttribute.MapInner)
            {
                return cgrNode;
            }

            return cgrNode
                .SyntaxNodes
                [cgrAttribute.InnerPosition]
                ;

        }

        public List<T> GetCGRValue<T>(SyntaxNode cgrNode, CGRAttribute cgrAttribute) where T : new()
        {

            return cgrNode
                .SyntaxNodes
                .Select(x => ApplyCGRProjection(x, cgrAttribute))
                .Select(x => MapTo<T>(x))
                .ToList()
                ;

        }

        public string GetTokenValue(SyntaxNode node, string tokenName)
        {

            return node
                .SyntaxNodes
                .Where(x => x.Rule.RuleType == RuleType.Token)
                .Single(x => x.Rule.Name.ToLower() == tokenName.ToLower())
                .Capture
                ;

        }

        public T GetFieldValue<T>(SyntaxNode node, string fieldName) where T : new()
        {

            var childNode = node
                .SyntaxNodes
                .Where(x => x.Rule.RuleType == RuleType.Form)
                .Single(x => x.Rule.Name.ToLower() == fieldName.ToLower())
                ;

            return MapTo<T>(childNode);

        }

    }
}
