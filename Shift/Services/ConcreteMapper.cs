using Shift.Domain;
using Shift.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concrete = Shift.Concrete;

namespace Shift.Services
{

    

    

    public class ConcreteMapper
    {

        public void MapSourceToApplication(Concrete.Source source, TypeContext typeContext)
        {

            var app = typeContext.Application;

            app.Name = "ShiftExample";

            foreach(var aspect in source.Aspects)
            {

                var key = typeContext.Tracker[(aspect.Value as Concrete.Identifiable).Identifier];

                Domain.Type type = aspect.Value switch
                {
                    Concrete.Data data => MapData(data, typeContext),
                    Concrete.Library library => MapLibrary(library, typeContext),
                    Concrete.Service service => MapService(service, typeContext),
                    _ => throw new Exception("What type of aspect is this?")
                };

                type.Source = TypeSource.User;

                typeContext.Application.AddType(type);


            }

        }

        public Data MapData(Concrete.Data source, TypeContext typeContext)
        {

            var data = new Data
            {
                Name = source.Identifier,
                Namespace = typeContext.Application.Name
            };

            data.Fields = source
                .Fields
                .Select(x => MapField(x, typeContext))
                .ToDictionary(x => x.Identifier)
                ;

            return data;

        }

        public Field MapField(Concrete.Field source, TypeContext typeContext)
        {

            var field = typeContext.Tracker.MapTypeDef<Field>(source.TypeDef);

            return field;

        }

        public Library MapLibrary(Concrete.Library source, TypeContext typeContext)
        {

            var library = new Library
            {
                Name = source.Identifier,
                Namespace = typeContext.Application.Name
            };

            library.Methods = source
                .Methods
                .Select(x => MapMethod(x, typeContext))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            return library;


        }

        public Service MapService(Concrete.Service source, TypeContext typeContext)
        {

            var service = new Service
            {
                Name = source.Identifier,
                Namespace = typeContext.Application.Name
            };

            service.Methods = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Method>()
                .Select(x => MapMethod(x, typeContext))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            service.Fields = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Field>()
                .Select(x => MapField(x, typeContext))
                .ToDictionary(x => x.Identifier)
                ;

            service.Constructors = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Constructor>()
                .Select(x => MapConstructor(x, service.Name, typeContext));
                ;

            return service;

        }

        public Method MapConstructor(Concrete.Constructor source, string serviceName, TypeContext typeContext)
        {

            var methodSource = new Concrete.Method
            {
                Signature = new Concrete.Signature
                {
                    TypeDef = new Concrete.TypeDef
                    {
                        Type = new Concrete.Type
                        {
                            IDENTIFIER = serviceName
                        },
                        Identifier = new Concrete.Identifier { IDENTIFIER = "this" }
                    },
                    Parameters = source.Parameters
                },
                Block = source.Block
            };

            return MapMethod(methodSource, typeContext);

        }

        public Method MapMethod(Concrete.Method source, TypeContext typeContext)
        {

            
            var signature = typeContext.Tracker.MapTypeDef<Signature>(source.Signature.TypeDef);

            var method = new Method
            {
                Signature = signature
            };

            if(source.Signature.Parameters is not null)
            {
                signature.Parameters.Add(MapParameter(source.Signature.Parameters.Parameter, typeContext));
                signature.Parameters.AddRange(source.Signature.Parameters.AdditionalParameters.Select(x => MapParameter(x, typeContext)));
            }

            method.Block = MapBlock(source.Block, typeContext);

            return method;

        }

        public Parameter MapParameter(Concrete.Parameter source, TypeContext typeContext)
        {

            var parameter = typeContext.Tracker.MapTypeDef<Parameter>(source.TypeDef);

            return parameter;

        }

        public Block MapBlock(Concrete.Block source, TypeContext typeContext)
        {

            var statements = source
                .Statements
                .Select(x => MapStatement(x, typeContext))
                .ToList()
                ;

            return new Block
            {
                Statements = statements
            };

        }

        public Statement MapStatement(Concrete.Statement statement, TypeContext typeContext)
        {

            return statement.Value switch
            {
                Concrete.ControlStatement control => MapControl(control, typeContext),
                Concrete.Declaration declaration => MapDeclaration(declaration, typeContext),
                Concrete.Assignment assignment => MapAssignment(assignment, typeContext),
                Concrete.ReturnExpression returnExpression => MapReturnExpression(returnExpression, typeContext),
                Concrete.ExpressionStatement expression => MapExpression(expression.Expression, typeContext),
                _ => null,
            };
        }

        public ControlStatement MapControl(Concrete.ControlStatement source, TypeContext typeContext)
        {

            return source.Value switch 
            { 
                Concrete.IfControl ifControl => MapIfControl(ifControl, typeContext),
                Concrete.WhileControl whileControl => MapWhileControl(whileControl, typeContext)
            };


        }

        public IfControl MapIfControl(Concrete.IfControl source, TypeContext typeContext)
        {

            var ifControl = new IfControl();

            ifControl.Condition = MapConditionalOrExpression(source.Condition, typeContext);
            ifControl.Block = MapBlock(source.Block, typeContext);

            return ifControl;

        }

        public WhileControl MapWhileControl(Concrete.WhileControl source, TypeContext typeContext)
        {

            var whileControl = new WhileControl();

            whileControl.Condition = MapConditionalOrExpression(source.Condition, typeContext);
            whileControl.Block = MapBlock(source.Block, typeContext);

            return whileControl;

        }


        public Declaration MapDeclaration(Concrete.Declaration source, TypeContext typeContext)
        {

            var declaration = new Declaration();

            declaration.TypeDefinition = typeContext.Tracker.MapTypeDef<TypeDefinition>(source.TypeDef);

            if(source.Initializer is not null)
            {
                declaration.InitializerExpression = MapExpression(source.Initializer.Expression, typeContext);
            }

            return declaration;
        }

        public Assignment MapAssignment(Concrete.Assignment source, TypeContext typeContext)
        {

            var assignment = new Assignment();

            //Probably need something similiar to the type table that allows me to reference the scope chain
            assignment.IdentifierChain.Add(new Identifier { Path = source.Accessor.Identifier.IDENTIFIER } );
            assignment.IdentifierChain.AddRange(source.Accessor.DotAccessors.Select(x => new Identifier { Path = x.IDENTIFIER }));

            assignment.Expression = MapExpression(source.Expression, typeContext);

            return assignment;
        }

        public ReturnExpression MapReturnExpression(Concrete.ReturnExpression source, TypeContext typeContext)
        {

            var returnExpression = new ReturnExpression();

            returnExpression.Expression = MapExpression(source.Expression, typeContext);

            return returnExpression;
        }

        public Expression MapExpression(Concrete.Expression source, TypeContext typeContext)
        {

            return source.Value switch
            {
                Concrete.ConditionalOrExpression condOrExpr => MapConditionalOrExpression(condOrExpr, typeContext)
            };
        }

        public ConditionalOrExpression MapConditionalOrExpression(Concrete.ConditionalOrExpression condOrExpr, TypeContext typeContext)
        {

            var expr = new ConditionalOrExpression();

            expr.ConditionalAndExpression = MapConditionalAndExpression(condOrExpr.ConditionalAndExpression, typeContext);
            expr.ConditionalAndExpressions = condOrExpr
                .ConditionalAndExpressions
                .Select(x => MapConditionalAndExpression(x, typeContext))
                .ToList()
                ;

            return expr;

        }

        public ConditionalAndExpression MapConditionalAndExpression(Concrete.ConditionalAndExpression condAndExpr, TypeContext typeContext)
        {

            var expr = new ConditionalAndExpression();

            expr.EqualityExpression = MapEqualityExpressionExpression(condAndExpr.EqualityExpression, typeContext);
            expr.EqualityExpressions = condAndExpr
                .EqualityExpressions
                .Select(x => MapEqualityExpressionExpression(x, typeContext))
                .ToList()
                ;

            return expr;

        }

        public EqualityExpression MapEqualityExpressionExpression(Concrete.EqualityExpression equalityExpression, TypeContext typeContext)
        {

            var expr = new EqualityExpression();

            expr.Left = MapRelationalExpression(equalityExpression.RelationalExpression, typeContext);
            
            if(equalityExpression.EqualityExpressionChain is not null)
            {
                expr.EqualityOperator = equalityExpression.EqualityExpressionChain.EqualityOperator.Value switch
                {
                    "==" => EqualityOperator.Equals,
                    "!=" => EqualityOperator.NotEquals
                };
                expr.Right = MapRelationalExpression(equalityExpression.EqualityExpressionChain.RelationalExpression, typeContext);
            }

            return expr;

        }

        public RelationalExpression MapRelationalExpression(Concrete.RelationalExpression relationshipExpression, TypeContext typeContext)
        {

            var expr = new RelationalExpression();

            expr.Left = MapAdditiveExpression(relationshipExpression.AdditiveExpression, typeContext);

            if (relationshipExpression.RelationalExpressionChain is not null)
            {
                expr.RelationalOperator = relationshipExpression.RelationalExpressionChain.RelationalOperator.Value switch
                {
                    "<=" => RelationalOperator.LessThanOrEqual,
                    "<" => RelationalOperator.LessThan,
                    ">=" => RelationalOperator.GreaterThanOrEqual,
                    ">" => RelationalOperator.GreaterThan,
                };
                expr.Right = MapAdditiveExpression(relationshipExpression.RelationalExpressionChain.AdditiveExpression, typeContext);
            }

            return expr;

        }

        public AdditiveExpression MapAdditiveExpression(Concrete.AdditiveExpression additiveExpr, TypeContext typeContext)
        {

            var expr = new AdditiveExpression();

            expr.MultiplicativeExpression = MapMultiplicativeExpression(additiveExpr.MultiplicativeExpression, typeContext);
            expr.MultiplicativeExpressions = additiveExpr
                .AdditiveExpressionChain
                .Select(x => (MapAdditiveOperator(x.AdditiveOperator), MapMultiplicativeExpression(x.MultiplicativeExpression, typeContext)))
                .ToList()
                ;

            return expr;

        }

        public AdditiveOperator MapAdditiveOperator(Concrete.AdditiveOperator source)
        {

            return source.Value switch
            {
                "+" => AdditiveOperator.Addition,
                "-" => AdditiveOperator.Subtraction
            };

        }

        public MultiplicativeExpression MapMultiplicativeExpression(Concrete.MultiplicativeExpression multiplicativeExpression, TypeContext typeContext)
        {

            var expr = new MultiplicativeExpression();

            expr.UnaryExpression = MapUnaryExpression(multiplicativeExpression.UnaryExpression, typeContext);
            expr.UnaryExpressions = multiplicativeExpression
                .MultiplicativeExpressionChain
                .Select(x => (MapMultiplicativeOperator(x.MultiplicativeOperator), MapUnaryExpression(x.UnaryExpression, typeContext)))
                .ToList()
                ;

            return expr;

        }
        public MultiplicativeOperator MapMultiplicativeOperator(Concrete.MultiplicativeOperator source)
        {

            return source.Value switch
            {                
                "*" => MultiplicativeOperator.Multiplication,
                "/" => MultiplicativeOperator.Division
            };

        }

        public UnaryExpression MapUnaryExpression(Concrete.UnaryExpression source, TypeContext typeContext)
        {

            var unaryExpression = new UnaryExpression();

            unaryExpression.MainExpression = MapMainExpression(source.MainExpression, typeContext);

            return unaryExpression;

        }

        public MainExpression MapMainExpression(Concrete.MainExpression source, TypeContext typeContext)
        {

            var mainExpression = new MainExpression();

            mainExpression.ExpressionStart = MapExpressionStart(source.ExpressionStart, typeContext);

            mainExpression.ExpressionChain = source
                .ExpressionChains
                .Select(x => MapExpressionChain(x, typeContext))
                .ToList()
                ;

            return mainExpression;

        }

        public ExpressionChain MapExpressionChain(Concrete.ExpressionChain source, TypeContext typeContext)
        {

            return source.Value switch
            {
                Concrete.MemberAccess memberAccess => MapIdentifier(memberAccess.Identifier, typeContext),
                Concrete.Invocation invocation => MapInvocation(invocation, typeContext)
            };

        }

        public ExpressionStart MapExpressionStart(Concrete.ExpressionStart source, TypeContext typeContext)
        {
            return source.Value switch
            {
                Concrete.Literal literal => MapLiteral(literal, typeContext),
                Concrete.Identifier identifier => MapIdentifier(identifier, typeContext),
                Concrete.ParensExpression parensExpression => MapParensExpression(parensExpression, typeContext),
                Concrete.NewExpression newExpression => MapNewExpression(newExpression, typeContext),
                Concrete.Invocation invocation => MapInvocation(invocation, typeContext),
                _ => null,
            };
        }

        public Literal MapLiteral(Concrete.Literal source, TypeContext typeContext)
        {
            return source.Value switch
            {
                Concrete.Boolean boolean => new Literal<bool> { Value = Convert.ToBoolean(boolean.Value) },
                Concrete.String s => new Literal<string> { Value = s.STRING },
                Control.TokenValue tokenValue => new Literal<int> { Value = Convert.ToInt32(tokenValue.Value) }
            };
        }

        public Identifier MapIdentifier(Concrete.Identifier source, TypeContext typeContext)
        {
            return new Identifier { Path = source.IDENTIFIER };
        }

        public ParensExpression MapParensExpression(Concrete.ParensExpression source, TypeContext typeContext)
        {
            return new ParensExpression
            {
                Expression = MapExpression(source.Expression, typeContext)
            };
        }

        public NewExpression MapNewExpression(Concrete.NewExpression source, TypeContext typeContext)
        {
            return new NewExpression
            { 
                Identifier = MapIdentifier(source.Identifier, typeContext),
                Invocation = MapInvocation(source.Invocation, typeContext)
            };

        }

        public Invocation MapInvocation(Concrete.Invocation source, TypeContext typeContext)
        {

            var invocation = new Invocation();

            if (source.Arguments is not null)
            {
                invocation.Arguments.Add(MapExpression(source.Arguments.Argument.Expression, typeContext));
                invocation.Arguments.AddRange(source.Arguments.AdditionalArguments.Select(x => MapExpression(x.Expression, typeContext)));
            }

            return invocation;

        }

    }
}
