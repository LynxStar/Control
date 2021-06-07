using Shift.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concrete = Shift.Concrete;

namespace Shift.Services
{

    public class ApplicationService
    {

        public Application MapSourceToApplication(Concrete.Source source)
        {

            var application = new Application();

            application.Name = "ShiftExample";

            foreach(var aspect in source.Aspects)
            {

                var key = application.Tracker[(aspect.Value as Concrete.Identifiable).Identifier];

                key.Source = TypeSource.User;

                Domain.Type type = aspect.Value switch
                {
                    Concrete.Data data => MapData(data, application),
                    Concrete.Library library => MapLibrary(library, application),
                    Concrete.Service service => MapService(service, application),
                    _ => throw new Exception("What type of aspect is this?")
                };

                application.AddType(type);


            }

            return application;

        }

        public Application LinkSchemaTrackedTypes(Application app, TypeService typeService)
        {

            var unknownTypes = app
                .Tracker
                .ConsumedTypes
                .Where(x => x.Value.Source is TypeSource.Unknown)
                .Where(x => x.Key != "var")//We'll infer var later
                .Select(x => x.Value)
                .ToList()
                ;

            foreach(var unknown in unknownTypes)
            {

                unknown.BackingType = typeService.RetrieveExternalType(unknown);
                unknown.Source = unknown.BackingType is SeedType
                    ? TypeSource.Seeded
                    : TypeSource.External
                    ;

            }

            app.Tracker.Resolver = (x) =>
            {
                var tracked = app.Tracker.InitialResolver(x);
                var exported = typeService.RetrieveExternalType(tracked);
                tracked.Source = TypeSource.External;

                return tracked;
            };

            return app;

        }

        public Data MapData(Concrete.Data source, Application app)
        {

            var data = new Data
            {
                Name = source.Identifier,
                Namespace = app.Name
            };

            data.Fields = source
                .Fields
                .Select(x => MapField(x, app))
                .ToDictionary(x => x.Identifier)
                ;

            return data;

        }

        public Field MapField(Concrete.Field source, Application app)
        {

            var field = app.Tracker.MapTypeDef<Field>(source.TypeDef);

            return field;

        }

        public Library MapLibrary(Concrete.Library source, Application app)
        {

            var library = new Library
            {
                Name = source.Identifier,
                Namespace = app.Name
            };

            library.Methods = source
                .Methods
                .Select(x => MapMethod(x, app))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            return library;


        }

        public Service MapService(Concrete.Service source, Application app)
        {

            var service = new Service
            {
                Name = source.Identifier,
                Namespace = app.Name
            };

            service.Methods = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Method>()
                .Select(x => MapMethod(x, app))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            service.Fields = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Field>()
                .Select(x => MapField(x, app))
                .ToDictionary(x => x.Identifier)
                ;

            service.Constructors = source
                .ServiceMembers
                .Select(x => x.Value)
                .OfType<Concrete.Constructor>()
                .Select(x => MapConstructor(x, service.Name, app));
                ;

            return service;

        }

        public Method MapConstructor(Concrete.Constructor source, string serviceName, Application app)
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

            return MapMethod(methodSource, app);

        }

        public Method MapMethod(Concrete.Method source, Application app)
        {

            
            var signature = app.Tracker.MapTypeDef<Signature>(source.Signature.TypeDef);

            var method = new Method
            {
                Signature = signature
            };

            if(source.Signature.Parameters is not null)
            {
                signature.Parameters.Add(MapParameter(source.Signature.Parameters.Parameter, app));
                signature.Parameters.AddRange(source.Signature.Parameters.AdditionalParameters.Select(x => MapParameter(x, app)));
            }

            method.Block = MapBlock(source.Block, app);

            return method;

        }

        public Parameter MapParameter(Concrete.Parameter source, Application app)
        {

            var parameter = app.Tracker.MapTypeDef<Parameter>(source.TypeDef);

            return parameter;

        }

        public Block MapBlock(Concrete.Block source, Application app)
        {

            var statements = source
                .Statements
                .Select(x => MapStatement(x, app))
                .ToList()
                ;

            return new Block
            {
                Statements = statements
            };

        }

        public Statement MapStatement(Concrete.Statement statement, Application app)
        {

            return statement.Value switch
            {
                Concrete.ControlStatement control => MapControl(control, app),
                Concrete.Declaration declaration => MapDeclaration(declaration, app),
                Concrete.Assignment assignment => MapAssignment(assignment, app),
                Concrete.ReturnExpression returnExpression => MapReturnExpression(returnExpression, app),
                Concrete.ExpressionStatement expression => MapExpression(expression.Expression, app),
                _ => null,
            };
        }

        public ControlStatement MapControl(Concrete.ControlStatement source, Application app)
        {

            return source.Value switch 
            { 
                Concrete.IfControl ifControl => MapIfControl(ifControl, app),
                Concrete.WhileControl whileControl => MapWhileControl(whileControl, app)
            };


        }

        public IfControl MapIfControl(Concrete.IfControl source, Application app)
        {

            var ifControl = new IfControl();

            ifControl.Condition = MapConditionalOrExpression(source.Condition, app);
            ifControl.Block = MapBlock(source.Block, app);

            return ifControl;

        }

        public WhileControl MapWhileControl(Concrete.WhileControl source, Application app)
        {

            var whileControl = new WhileControl();

            whileControl.Condition = MapConditionalOrExpression(source.Condition, app);
            whileControl.Block = MapBlock(source.Block, app);

            return whileControl;

        }


        public Declaration MapDeclaration(Concrete.Declaration source, Application app)
        {

            var declaration = new Declaration();

            declaration.TypeDefinition = app.Tracker.MapTypeDef<TypeDefinition>(source.TypeDef);

            if(source.Initializer is not null)
            {
                declaration.InitializerExpression = MapExpression(source.Initializer.Expression, app);
            }

            return declaration;
        }

        public Assignment MapAssignment(Concrete.Assignment source, Application app)
        {

            var assignment = new Assignment();

            //Probably need something similiar to the type table that allows me to reference the scope chain
            assignment.IdentifierChain.Add(new Identifier { Path = source.Accessor.Identifier.IDENTIFIER } );
            assignment.IdentifierChain.AddRange(source.Accessor.DotAccessors.Select(x => new Identifier { Path = x.IDENTIFIER }));

            assignment.Expression = MapExpression(source.Expression, app);

            return assignment;
        }

        public ReturnExpression MapReturnExpression(Concrete.ReturnExpression source, Application app)
        {

            var returnExpression = new ReturnExpression();

            returnExpression.Expression = MapExpression(source.Expression, app);

            return returnExpression;
        }

        public Expression MapExpression(Concrete.Expression source, Application app)
        {

            return source.Value switch
            {
                Concrete.ConditionalOrExpression condOrExpr => MapConditionalOrExpression(condOrExpr, app)
            };
        }

        public ConditionalOrExpression MapConditionalOrExpression(Concrete.ConditionalOrExpression condOrExpr, Application app)
        {

            var expr = new ConditionalOrExpression();

            expr.ConditionalAndExpression = MapConditionalAndExpression(condOrExpr.ConditionalAndExpression, app);
            expr.ConditionalAndExpressions = condOrExpr
                .ConditionalAndExpressions
                .Select(x => MapConditionalAndExpression(x, app))
                .ToList()
                ;

            return expr;

        }

        public ConditionalAndExpression MapConditionalAndExpression(Concrete.ConditionalAndExpression condAndExpr, Application app)
        {

            var expr = new ConditionalAndExpression();

            expr.EqualityExpression = MapEqualityExpressionExpression(condAndExpr.EqualityExpression, app);
            expr.EqualityExpressions = condAndExpr
                .EqualityExpressions
                .Select(x => MapEqualityExpressionExpression(x, app))
                .ToList()
                ;

            return expr;

        }

        public EqualityExpression MapEqualityExpressionExpression(Concrete.EqualityExpression equalityExpression, Application app)
        {

            var expr = new EqualityExpression();

            expr.Left = MapRelationalExpression(equalityExpression.RelationalExpression, app);
            
            if(equalityExpression.EqualityExpressionChain is not null)
            {
                expr.EqualityOperator = equalityExpression.EqualityExpressionChain.EqualityOperator.Value switch
                {
                    "==" => EqualityOperator.Equals,
                    "!=" => EqualityOperator.NotEquals
                };
                expr.Right = MapRelationalExpression(equalityExpression.EqualityExpressionChain.RelationalExpression, app);
            }

            return expr;

        }

        public RelationalExpression MapRelationalExpression(Concrete.RelationalExpression relationshipExpression, Application app)
        {

            var expr = new RelationalExpression();

            expr.Left = MapAdditiveExpression(relationshipExpression.AdditiveExpression, app);

            if (relationshipExpression.RelationalExpressionChain is not null)
            {
                expr.RelationalOperator = relationshipExpression.RelationalExpressionChain.RelationalOperator.Value switch
                {
                    "<=" => RelationalOperator.LessThanOrEqual,
                    "<" => RelationalOperator.LessThan,
                    ">=" => RelationalOperator.GreaterThanOrEqual,
                    ">" => RelationalOperator.GreaterThan,
                };
                expr.Right = MapAdditiveExpression(relationshipExpression.RelationalExpressionChain.AdditiveExpression, app);
            }

            return expr;

        }

        public AdditiveExpression MapAdditiveExpression(Concrete.AdditiveExpression additiveExpr, Application app)
        {

            var expr = new AdditiveExpression();

            expr.MultiplicativeExpression = MapMultiplicativeExpression(additiveExpr.MultiplicativeExpression, app);
            expr.MultiplicativeExpressions = additiveExpr
                .AdditiveExpressionChain
                .Select(x => (MapAdditiveOperator(x.AdditiveOperator), MapMultiplicativeExpression(x.MultiplicativeExpression, app)))
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

        public MultiplicativeExpression MapMultiplicativeExpression(Concrete.MultiplicativeExpression multiplicativeExpression, Application app)
        {

            var expr = new MultiplicativeExpression();

            expr.UnaryExpression = MapUnaryExpression(multiplicativeExpression.UnaryExpression, app);
            expr.UnaryExpressions = multiplicativeExpression
                .MultiplicativeExpressionChain
                .Select(x => (MapMultiplicativeOperator(x.MultiplicativeOperator), MapUnaryExpression(x.UnaryExpression, app)))
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

        public UnaryExpression MapUnaryExpression(Concrete.UnaryExpression source, Application app)
        {

            var unaryExpression = new UnaryExpression();

            unaryExpression.MainExpression = MapMainExpression(source.MainExpression, app);

            return unaryExpression;

        }

        public MainExpression MapMainExpression(Concrete.MainExpression source, Application app)
        {

            var mainExpression = new MainExpression();

            mainExpression.ExpressionStart = MapExpressionStart(source.ExpressionStart, app);

            mainExpression.ExpressionChain = source
                .ExpressionChains
                .Select(x => MapExpressionChain(x, app))
                .ToList()
                ;

            return mainExpression;

        }

        public ExpressionChain MapExpressionChain(Concrete.ExpressionChain source, Application app)
        {

            return source.Value switch
            {
                Concrete.MemberAccess memberAccess => MapIdentifier(memberAccess.Identifier, app),
                Concrete.Invocation invocation => MapInvocation(invocation, app)
            };

        }

        public ExpressionStart MapExpressionStart(Concrete.ExpressionStart source, Application app)
        {
            return source.Value switch
            {
                Concrete.Literal literal => MapLiteral(literal, app),
                Concrete.Identifier identifier => MapIdentifier(identifier, app),
                Concrete.ParensExpression parensExpression => MapParensExpression(parensExpression, app),
                Concrete.NewExpression newExpression => MapNewExpression(newExpression, app),
                Concrete.Invocation invocation => MapInvocation(invocation, app),
                _ => null,
            };
        }

        public Literal MapLiteral(Concrete.Literal source, Application app)
        {
            return source.Value switch
            {
                Concrete.Boolean boolean => new Literal<bool> { Value = Convert.ToBoolean(boolean.Value) },
                Concrete.String s => new Literal<string> { Value = s.STRING },
                Control.TokenValue tokenValue => new Literal<int> { Value = Convert.ToInt32(tokenValue.Value) }
            };
        }

        public Identifier MapIdentifier(Concrete.Identifier source, Application app)
        {
            return new Identifier { Path = source.IDENTIFIER };
        }

        public ParensExpression MapParensExpression(Concrete.ParensExpression source, Application app)
        {
            return new ParensExpression
            {
                Expression = MapExpression(source.Expression, app)
            };
        }

        public NewExpression MapNewExpression(Concrete.NewExpression source, Application app)
        {
            return new NewExpression
            { 
                Identifier = MapIdentifier(source.Identifier, app),
                Invocation = MapInvocation(source.Invocation, app)
            };

        }

        public Invocation MapInvocation(Concrete.Invocation source, Application app)
        {

            var invocation = new Invocation();

            if (source.Arguments is not null)
            {
                invocation.Arguments.Add(MapExpression(source.Arguments.Argument.Expression, app));
                invocation.Arguments.AddRange(source.Arguments.AdditionalArguments.Select(x => MapExpression(x.Expression, app)));
            }

            return invocation;

        }

    }
}
