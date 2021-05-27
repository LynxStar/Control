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

            application.Name = "Test";

            foreach(var aspect in source.Aspects)
            {

                var meta = application[(aspect.Value as Concrete.Identifiable).Identifier];

                meta.Source = TypeSource.UserDefined;

                meta.BackingType = aspect.Value switch
                {
                    Concrete.Data data => MapData(data, application),
                    Concrete.Library library => MapLibrary(library, application),
                    Concrete.Service service => MapService(service, application),
                    _ => throw new Exception("What type of aspect is this?")
                };

            }

            return application;

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

            var field = app.MapTypeDef<Field>(source.TypeDef);

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
                .Select(x => MapConstructor(x, service.Name, app))
                .GroupBy(x => x.Signature.Identifier)
                .ToDictionary(x => x.Key, x => x.ToList())
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

            
            var signature = app.MapTypeDef<Signature>(source.Signature.TypeDef);

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

            var parameter = app.MapTypeDef<Parameter>(source.TypeDef);

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
                Concrete.Declaration declaration => MapDeclaration(declaration, app),
                Concrete.Assignment assignment => MapAssignment(assignment, app),
                Concrete.ReturnExpression returnExpression => MapReturnExpression(returnExpression, app),
                Concrete.Expression expression => MapExpression(expression, app),
                _ => null,
            };
        }

        public Declaration MapDeclaration(Concrete.Declaration source, Application app)
        {

            var declaration = new Declaration();

            declaration.TypeDefinition = app.MapTypeDef<TypeDefinition>(source.TypeDef);

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
                Concrete.BinaryExpression binary => MapBinaryExpression(binary, app),
                Concrete.UnaryExpression unary => MapUnaryExpression(unary, app)
            };
        }

        public BinaryExpression MapBinaryExpression(Concrete.BinaryExpression source, Application app)
        {

            var binaryExpression = new BinaryExpression();

            binaryExpression.Left = MapUnaryExpression(source.Left, app);
            binaryExpression.Operator = MapOperator(source.Operator, app);
            binaryExpression.Right = MapUnaryExpression(source.Right, app);

            return binaryExpression;

        }

        public Operator MapOperator(Concrete.Operator source, Application app)
        {

            return source.Value switch
            {
                "==" => Operator.Equals,
                "!=" => Operator.NotEquals
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
