using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
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

                if(property.PropertyType == typeof(string))
                {
                    value = GetTokenValue(node, property.Name);
                }

                else if(property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var method = typeof(ConcreteService).GetMethod("GetCGRValue");

                    var listInnerType = property.PropertyType.GenericTypeArguments.First();

                    var genericMethod = method.MakeGenericMethod(listInnerType);
                    value = genericMethod.Invoke(this, new object[] { node, property.Name });
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

        public List<T> GetCGRValue<T>(SyntaxNode node, string fieldName) where T : new()
        {

            return node
                .SyntaxNodes
                .Where(x => x.Rule.Name.Contains("CGR"))
                .Where(x => x.SyntaxNodes.Count() == 1)
                .Where(x => !x.SyntaxNodes.Any(x => x.Rule.Name.ToLower() != fieldName.ToLower()))
                .SelectMany(x => x.SyntaxNodes)
                .Select(x => MapTo<T>(x))
                .ToList()
                ;

        }

    }
}
