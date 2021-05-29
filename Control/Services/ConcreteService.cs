using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{

    public class TargetContext
    {
        public SyntaxNode TargetNode { get; set; }
        public Type DestinationType { get; set; }
        public CGRAttribute CGRAttribute { get; set; }
        public bool IsList { get; set; }

        public TargetContext WithNode(SyntaxNode targetNode)
        {

            return new TargetContext
            {
                TargetNode = targetNode,
                DestinationType = DestinationType,
                CGRAttribute = CGRAttribute,
                IsList = IsList
            };

        }

        public TargetContext WithType(Type type)
        {
            return new TargetContext
            {
                TargetNode = TargetNode,
                DestinationType = type,
                CGRAttribute = CGRAttribute,
                IsList = IsList
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

                object value = null;

                if(context.TargetNode.SelectedOption is CaptureGroup cg)
                {   

                    if(cg.Modifier == CaptureModifier.NoneToOne && !context.TargetNode.SyntaxNodes.Any())
                    {
                        value = null;
                    }
                    else
                    {
                        var method = typeof(ConcreteService).GetMethod("MapCGR");
                        var genericMethod = method.MakeGenericMethod(context.DestinationType);
                        value = genericMethod.Invoke(this, new object[] { context });
                    }

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

            var context = new TargetContext
            {
                TargetNode = node,
                DestinationType = property.PropertyType,
                IsList = property.PropertyType.IsList(),
            };

            context.CGRAttribute = property
                    .GetCustomAttributes(typeof(CGRAttribute), true)
                    .OfType<CGRAttribute>()
                    .SingleOrDefault()
                    ;

            context.TargetNode = GetTargetNode(context, property);

            if(context.IsList)
            {
                context.DestinationType = property
                    .PropertyType
                    .GenericTypeArguments
                    .Single()
                    ;
            }

            return context;

        }

        public SyntaxNode GetTargetNode(TargetContext context, PropertyInfo property)
        {

            //Explicit Attribute Metadata
            if(context.CGRAttribute is not null || context.IsList)
            {   

                return context
                    .TargetNode
                    .SyntaxNodes
                    .Where(x => x.SelectedOption is CaptureGroup)
                    .ToList()
                    [context.CGRAttribute?.Position ?? 0]
                    ;

            }

            //Implicit rule name + string maps to token

            var filterRuleType = context.DestinationType == typeof(string)
                ? RuleType.Token
                : RuleType.Form
                ;

            var instanceAttr = property
                    .GetCustomAttributes(typeof(Instance), true)
                    .OfType<Instance>()
                    .SingleOrDefault()
                    ;

            var instance = instanceAttr is null
                ? 0
                : instanceAttr.Position
                ;

            var filter = context
                .TargetNode
                .SyntaxNodes
                .Where(x => x.Rule.RuleType == filterRuleType)
                ;

            return filter
                .Where(x => x.Rule.SnakeCase() == property.GetFormName())
                .Skip(instance)
                .First()
                ;
        }

        public object MapCGR<T>(TargetContext context) where T : class
        {

            if(context.IsList)
            {

                return context
                    .TargetNode
                    .SyntaxNodes
                    .Select(x => MapCGRInsides<T>(context.WithNode(x)))
                    .ToList()
                    ;

            }

            return MapCGRInsides<T>(context);

        }

        public T MapCGRInsides<T>(TargetContext context) where T : class
        {

            var cg = context.TargetNode.SelectedOption as CaptureGroup;

            var allowNone = cg.Modifier is CaptureModifier.Optional or CaptureModifier.NoneToOne;

            if (allowNone && !context.TargetNode.SyntaxNodes.Any())
            {
                return null;
            }

            var mapNode = GetMapNode(context);

            return MapDirectly(context.WithNode(mapNode)) as T;

        }

        private static SyntaxNode GetMapNode(TargetContext context)
        {

            //Explicit or implicit
            var mapDirect = context.CGRAttribute?.MapDirect ?? context.TargetNode.SyntaxNodes.Count() == 1;
            var directPosition = context.CGRAttribute?.InnerPosition ?? 0;

            var mapNode = mapDirect
                ? context.TargetNode.SyntaxNodes[directPosition]
                : context.TargetNode
                ;

            return mapNode;

        }



        public TargetContext ApplyOptionLogic(TargetContext context)
        {

            var optionsBase = context
                .DestinationType
                .BaseType
                ;

            //There are no options
            if (optionsBase?.IsAssignableTo(typeof(IOption)) == false)
            {
                return context;
            }

            //Options possible but no entries, so never materialized into an option
            if (context.TargetNode is null)
            {
                return context;
            }

            var option = context.TargetNode.SelectedOption.ToString();

            var isSingleToken = context
                .TargetNode
                .SelectedOption
                .Clauses
                .SingleOrDefault()
                ?.Reference
                ?.RuleType == RuleType.Token
                ;

            var hasMatchingType = optionsBase
                .GenericTypeArguments
                .ToDictionary(x => x.GetFormName())
                .Count(x => x.Key.ToLower() == option) == 1
                ;

            Type usedOption = null;

            if(hasMatchingType)
            {
                usedOption = optionsBase
                .GenericTypeArguments
                .ToDictionary(x => x.GetFormName())
                .Single(x => x.Key.ToLower() == option)
                .Value
                ;
            }
            else if(isSingleToken)
            {
                usedOption = typeof(TokenValue);
            }
            else
            {

                var typeArgs = optionsBase
                    .GenericTypeArguments
                    ;

                var formNames = typeArgs
                    .ToDictionary(x => x.GetFormName())
                    ;

                var count = formNames
                    .Count(x => x.Key.ToLower() == option)
                    ;

                throw new Exception("Unmatched Option");
            }

            //This currently assumes that an option consists of a single clause
            var optionNode = context.TargetNode.SyntaxNodes.Single();

            return context
                .WithNode(optionNode)
                .WithType(usedOption)
                ;

        }

        public object MapDirectly(TargetContext context)
        {

            if (context.DestinationType == typeof(string))
            {
                return context.TargetNode.Capture;
            }

            else
            {

                var originalType = context.DestinationType;

                context = ApplyOptionLogic(context);

                object value = null;

                if(context.DestinationType.BaseType?.IsAssignableTo(typeof(IOption)) == true)
                {
                    value = MapDirectly(context);
                }
                else if(context.DestinationType.IsAssignableTo(typeof(TokenValue)))
                {

                    var tokenValue = Activator.CreateInstance(context.DestinationType) as TokenValue;

                    tokenValue.Token = context.TargetNode.Rule.Name;
                    tokenValue.Value = context.TargetNode.TokenValue;

                    value = tokenValue;
                }
                else
                {
                    var method = typeof(ConcreteService).GetMethod("MapNodeToObject");
                    var genericMethod = method.MakeGenericMethod(context.DestinationType);
                    value = genericMethod.Invoke(this, new object[] { context.TargetNode });
                }

                if(originalType != context.DestinationType)
                {
                    var optionsWrapper = Activator.CreateInstance(originalType) as IOption;
                    optionsWrapper.Value = value;
                    value = optionsWrapper;
                }

                return value;

            }

        }

    }
}
