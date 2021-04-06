using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{

    public interface Options { }

    public interface Options<T1, T2, T3, T4> : Options { }

    public class TargetContext
    {
        public SyntaxNode TargetNode { get; set; }
        public Type DestinationType { get; set; }
        public CGRAttribute CGRAttribute { get; set; }

        public TargetContext WithNode(SyntaxNode targetNode)
        {

            return new TargetContext
            {
                TargetNode = targetNode,
                DestinationType = DestinationType,
                CGRAttribute = CGRAttribute
            };

        }

        public TargetContext WithType(Type type)
        {
            return new TargetContext
            {
                TargetNode = TargetNode,
                DestinationType = type,
                CGRAttribute = CGRAttribute
            };
        }

    }

    public class ConcreteService
    {

        public T MapNodeToObject<T>(SyntaxNode node) where T : new()
        {

            var properties = typeof(T).GetProperties();

            T result = new T();

            foreach (var property in properties)
            {

                var context = BuildPropertyContext(node, property);

                //Option goes here

                object value = null;

                if(context.TargetNode.SelectedOption is CaptureGroup)
                {

                    var innerType = context.DestinationType.IsList()
                        ? context.DestinationType.GenericTypeArguments.First()
                        : context.DestinationType
                        ;

                    var method = typeof(ConcreteService).GetMethod("MapCGR");
                    var genericMethod = method.MakeGenericMethod(context.DestinationType, innerType);
                    value = genericMethod.Invoke(this, new object[] { context });

                }
                else
                {
                    value = MapDirectly(context);
                }

                property.SetValue(result, value);

            }

            return result;
        }

        public TargetContext BuildPropertyContext(SyntaxNode node, PropertyInfo property)
        {

            var cgrAttribute = property
                    .GetCustomAttributes(typeof(CGRAttribute), true)
                    .OfType<CGRAttribute>()
                    .SingleOrDefault()
                    ;

            var targetNode = GetTargetNode(node, property, cgrAttribute);

            return new TargetContext
            {
                TargetNode = targetNode,
                DestinationType = property.PropertyType,
                CGRAttribute = cgrAttribute
            };

        }

        public SyntaxNode GetTargetNode(SyntaxNode node, PropertyInfo property, CGRAttribute cgrAttribute)
        {

            //Explicit Attribute Metadata
            if(cgrAttribute is not null || property.PropertyType.IsList())
            {   

                return node
                    .SyntaxNodes
                    .Where(x => x.SelectedOption is CaptureGroup)
                    .ToList()
                    [cgrAttribute?.Position ?? 0]
                    ;

            }

            //Implicit rule name + string maps to token

            var filterRuleType = property.PropertyType == typeof(string)
                ? RuleType.Token
                : RuleType.Form
                ;

            return node
                .SyntaxNodes
                .Where(x => x.Rule.RuleType == filterRuleType)
                .Single(x => x.Rule.Name.ToLower() == property.Name.ToLower())
                ;
        }

        public object MapCGR<T,IT>(TargetContext context) where T : class where IT : class
        {

            if(typeof(T).IsList())
            {

                return context
                    .TargetNode
                    .SyntaxNodes
                    .Select(x => MapCGRInsides<IT>(context.WithNode(x).WithType(typeof(IT))))
                    .ToList()
                    ;

            }

            return MapCGRInsides<T>(context);

        }

        public T MapCGRInsides<T>(TargetContext context) where T : class
        {

            //Explicit or implicit
            var mapDirect = context.CGRAttribute?.MapDirect ?? context.TargetNode.SyntaxNodes.Count() == 1;
            var directPosition = context.CGRAttribute?.InnerPosition ?? 0;

            var mapNode = mapDirect
                ? context.TargetNode.SyntaxNodes[directPosition]
                : context.TargetNode
                ;

            return MapDirectly(context.WithNode(mapNode)) as T;

        }

        public object MapDirectly(TargetContext context)
        {

            if (context.DestinationType == typeof(string))
            {
                return context.TargetNode.Capture;
            }

            else
            {
                var method = typeof(ConcreteService).GetMethod("MapNodeToObject");
                var genericMethod = method.MakeGenericMethod(context.DestinationType);
                return genericMethod.Invoke(this, new object[] { context.TargetNode });
            }

        }

    }
}
