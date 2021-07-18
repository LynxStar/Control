using Shift.Domain;
using Shift.Intermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Services
{

    public class TypeEnforcer
    {

        public void TypeCheckApplication(TypeContext typeContext)
        {

            var scope = new LexicalScope
            {
                TypeContext = typeContext,
                Scope = typeContext
                    .Application
                    .Types
                    .ToDictionary(x => x.Key, x => typeContext.Tracker[x.Value.Name])
            };

            typeContext
                .Application
                .Types
                .Select(x => x.Value)
                .ToList()
                .ForEach(x => ProcessType(x, scope))
                ;
                    
        }

        public void ProcessType(Domain.Type type, LexicalScope scope)
        {

            scope = scope.StartChildScope();

            if (type is HasFields hasFields)
            {
                foreach (var field in hasFields.Fields)
                {
                    scope.Scope.Add(field.Key, field.Value.Type);
                }
            }

            //Merge list of things to check
            List<Method> methods = new List<Method>();

            if(type is HasMethods methodSource)
            {

                foreach(var method in methodSource.Methods.SelectMany(x => x.Value))
                {
                    scope.Scope.Add(method.Signature.Identifier, method.Signature.Type);
                    methods.Add(method);
                }

            }
            if(type is HasConstructors constructorSource)
            {

                var constructors = constructorSource
                    .Constructors
                    ;

                methods.AddRange(constructors);

            }

            methods
                .ForEach(x => ProcessMethods(x, scope))
                ;

        }

        public void ProcessMethods(Method method, LexicalScope scope)
        {

            var returnType = method.Signature.Type;

            var initialVars = method//A method's initial scope is it's parameters
                .Signature
                .Parameters
                .ToDictionary(x => x.Identifier, x => x.Type)
                ;

            scope = scope.StartChildScope();
            scope.Scope = initialVars;
            scope.ReturnType = method.Signature.Type;

            ProcessBlock(method.Block, scope);

        }

        public class LexicalScope
        {

            public TypeContext TypeContext { get; set; }
            public LexicalScope Parent { get; set; }


            public Dictionary<string, TrackedType> Scope = new Dictionary<string, TrackedType>();
            public TrackedType ReturnType;

            public void Add(TypeDefinition typeDef)
            {
                Scope.Add(typeDef.Identifier, typeDef.Type);
            }

            public TrackedType this[string identifier]
            {
                get
                {

                    if(Scope.ContainsKey(identifier))
                    {
                        return Scope[identifier];
                    }

                    return Parent is not null
                        ? Parent[identifier]
                        : throw new Exception("Invalid identifier")
                        ;

                }
            }

            public LexicalScope StartChildScope()
            {
                return new LexicalScope
                {
                    TypeContext = TypeContext,
                    Parent = this,
                    ReturnType = ReturnType
                };
            }

        }


        public void ProcessBlock(Block block, LexicalScope scope)
        {

            foreach(var statement in block.Statements)
            {
                ProcessStatement(statement, scope);
            }

        }

        public void ProcessStatement(Statement statement, LexicalScope scope)
        {

            switch(statement)
            {
                case ControlStatement control:
                    ProcessControlStatement(control, scope);
                    break;
                case Declaration declaration:
                    ProcessDeclaration(declaration, scope);
                    break;
                case Assignment assignment:
                    ProcessAssignment(assignment, scope);
                    break;
                case ReturnExpression returnExpression:
                    ProcessReturnExpression(returnExpression, scope);
                    break;
                case Expression expression:
                    ProcessExpression(expression, scope);
                    break;
            }
        }

        public void ProcessControlStatement(ControlStatement control, LexicalScope scope)
        {

            var conditionType = ProcessExpression(control.Condition, scope);

            if(conditionType != scope.TypeContext.Tracker["Boolean"])
            {
                throw new Exception("condition must be boolean");
            }

            var childScope = scope.StartChildScope();

            ProcessBlock(control.Block, childScope);

        }

        public void ProcessDeclaration(Declaration declaration, LexicalScope scope)
        {

            
            if(declaration.InitializerExpression is not null)
            {

                var expressionType = ProcessExpression(declaration.InitializerExpression, scope);

                if(declaration.TypeDefinition.Type.Name == "var")
                {
                    declaration.TypeDefinition.Type = expressionType;
                }

                if(declaration.TypeDefinition.Type != expressionType)
                {
                    throw new Exception("Type mismatch");
                }

            }

            if (declaration.TypeDefinition.Type.Name == "var" && declaration.InitializerExpression is not null)
            {
                throw new Exception("var's type cannot be inferred without an initializer expression");
            }

            scope.Add(declaration.TypeDefinition);

        }

        public void ProcessAssignment(Assignment assignment, LexicalScope scope)
        {

            var referenceType = ResolveIdentifierChainType(assignment.IdentifierChain, scope);
            var assignedType = ProcessExpression(assignment.Expression, scope);

            if(referenceType != assignedType)
            {
                throw new Exception("You cannot assign the wrong type to this");
            }

        }

        public TrackedType ResolveIdentifierChainType(List<Identifier> identifiers, LexicalScope scope)
        {

            var currentNode = scope[identifiers.First().Path];

            foreach(var identifier in identifiers.Skip(1))
            {
                currentNode = ResolveTypeMemberAccess(currentNode, identifier.Path);
            }

            return currentNode;

        }

        public TrackedType ResolveTypeMemberAccess(TrackedType trackedType, string identifier)
        {

            var type = trackedType.BackingType;

            if (type is Data data)
            {
                if(data.Fields.ContainsKey(identifier))
                {
                    return data.Fields[identifier].Type;
                }
                else
                {
                    throw new Exception("Unknown identifier");
                }

            }
            else if (type is Service service)
            {

                if (service.Fields.ContainsKey(identifier))
                {
                    return service.Fields[identifier].Type;
                }
                else
                {
                    throw new Exception("Unknown identifier");
                }
            }
            else
            {
                throw new Exception("So libraries don't have fields and are readonly. Go honk yourself");
            }

        }

        public void ProcessReturnExpression(ReturnExpression returnExpression, LexicalScope scope)
        {

            ProcessExpression(returnExpression.Expression, scope);

            if(returnExpression.Expression.Output != scope.ReturnType)
            {
                throw new Exception("Return exception type mismatch");
            }

        }

        public TrackedType ProcessExpression(Expression expression, LexicalScope scope)
        {

            //Will this every trigger? Why would we need to pass an expression twice?
            if(expression.Output is not null)
            {
                return expression.Output;
            }

            //I think this is useful for the transpiler
            expression.Output = expression switch
            {
                ConditionalOrExpression conditionalOrExpression => ProcessConditionalOrExpressionType(conditionalOrExpression, scope),
            };

            return expression.Output;

        }

        public TrackedType ProcessConditionalOrExpressionType(ConditionalOrExpression conditionalOrExpression, LexicalScope scope)
        {

            var expressionType = ProcessConditionalAndExpressionType(conditionalOrExpression.ConditionalAndExpression, scope);

            foreach (var multExpr in conditionalOrExpression.ConditionalAndExpressions)
            {
                throw new Exception("|| isn't a supported operator yet");
            }

            return expressionType;

        }

        public TrackedType ProcessConditionalAndExpressionType(ConditionalAndExpression conditionalAndExpression, LexicalScope scope)
        {

            var expressionType = ProcessEqualityExpressionType(conditionalAndExpression.EqualityExpression, scope);

            foreach (var multExpr in conditionalAndExpression.EqualityExpressions)
            {
                throw new Exception("&& isn't a supported operator yet");
            }

            return expressionType;

        }

        public TrackedType ProcessEqualityExpressionType(EqualityExpression equalityExpression, LexicalScope scope)
        {

            var expressionType = ProcessRelationalExpressionType(equalityExpression.Left, scope);

            if (equalityExpression.Right is not null)
            {
                throw new Exception("== and != aren't supported operators yet");
            }

            return expressionType;

        }

        public TrackedType ProcessRelationalExpressionType(RelationalExpression relationalExpression, LexicalScope scope)
        {

            var expressionType = ProcessAdditiveExpressionType(relationalExpression.Left, scope);

            if (relationalExpression.Right is not null)
            {
                throw new Exception(">, >=, <, and <= aren't supported operators yet");
            }

            return expressionType;

        }

        public TrackedType ProcessAdditiveExpressionType(AdditiveExpression additiveExpression, LexicalScope scope)
        {

            var expressionType = ProcessMultiplicativeExpressionType(additiveExpression.MultiplicativeExpression, scope);

            foreach (var multExpr in additiveExpression.MultiplicativeExpressions)
            {
                throw new Exception("+ and - aren't supported operators yet");
            }

            return expressionType;

        }

        public TrackedType ProcessMultiplicativeExpressionType(MultiplicativeExpression multiplicativeExpression, LexicalScope scope)
        {

            var expressionType = ProcessUnaryExpressionType(multiplicativeExpression.UnaryExpression, scope);

            foreach(var multExpr in multiplicativeExpression.UnaryExpressions)
            {
                throw new Exception("* and / aren't supported operators yet");
            }

            return expressionType;

        }

        public TrackedType ProcessUnaryExpressionType(UnaryExpression unaryExpression, LexicalScope scope)
        {

            return ProcessMainExpressionType(unaryExpression.MainExpression, scope);

        }

        public TrackedType ProcessMainExpressionType(MainExpression mainExpression, LexicalScope scope)
        {

            var expressionChainType = ProcessExpressionStartType(mainExpression.ExpressionStart, scope);

            var previousIdentifier = mainExpression.ExpressionStart is Identifier ident
                ? ident.Path
                : string.Empty
                ;

            foreach(var link in mainExpression.ExpressionChain)
            {

                if(link is Identifier link_identifier)
                {
                    expressionChainType = ResolveTypeMemberAccess(expressionChainType, link_identifier.Path);
                    previousIdentifier = link_identifier.Path;
                }
                else if (link is Invocation invocation)
                {

                    if(previousIdentifier == string.Empty)
                    {
                        throw new Exception("Methods aren't supported as types yet");
                    }

                    var invocationMethod = RetrieveMethodInvocation(previousIdentifier, invocation, scope);
                    previousIdentifier = string.Empty;
                }

            }

            return expressionChainType;

        }

        public TrackedType ProcessExpressionStartType(ExpressionStart expressionStart, LexicalScope scope)
        {
            return expressionStart switch
            {
                Literal literal => ProcessLiteralType(literal, scope),
                Identifier identifier => ProcessIdentifierType(identifier, scope),
                ParensExpression parens => ProcessExpression(parens.Expression, scope),
                NewExpression newExpression => ProcessNewExpressionType(newExpression, scope),
                //Invocation invocation => BuildInvocationSource(invocation)
            };
        }

        public TrackedType ProcessLiteralType(Literal literal, LexicalScope scope)
        {

            return literal switch
            {
                Literal<int> => scope.TypeContext.Tracker["int"],
                Literal<bool> => scope.TypeContext.Tracker["bool"],
                Literal<string> => scope.TypeContext.Tracker["string"]
            };
        }

        public TrackedType ProcessIdentifierType(Identifier identifier, LexicalScope scope)
        {

            return scope[identifier.Path];

        }

        public TrackedType ProcessNewExpressionType(NewExpression newExpression, LexicalScope scope)
        {

            var newType = scope.TypeContext.Tracker[newExpression.Identifier.Path];

            //Match the parameters and make sure they are valid

            var constructors = (newType as HasConstructors)
                .Constructors
                .GroupBy(x => "this")
                .ToDictionary(x => x.Key, x => x.ToList())
                ;

            var argumentTypes = newExpression
                .Invocation
                .Arguments
                .Select(x => ProcessExpression(x, scope))
                ;

            var constructor = MatchMethod("this", argumentTypes, constructors);

            if(constructor is null)
            {
                throw new Exception("No constructor that matches the invocation argument types can be found");
            }

            if(constructor.Signature.Type != newType)
            {
                throw new Exception("Your constructor somehow didn't construct it's right type. I don't know");
            }

            return newType;
            
        }

        public Method RetrieveMethodInvocation(string identifier, Invocation invocation, LexicalScope scope)
        {

            var sourceType = scope[identifier];

            var methods = (sourceType as HasMethods)
                .Methods
                ;

            var argumentTypes = invocation
                .Arguments
                .Select(x => ProcessExpression(x, scope))
                ;

            var method = MatchMethod(identifier, argumentTypes, methods);

            return method;

        }

        public Method MatchMethod(string name, IEnumerable<TrackedType> arguments, Dictionary<string, List<Method>> methods)
        {

            if(!methods.ContainsKey(name))
            {
                throw new Exception("This method doesn't exist on the type");
            }

            return methods
                [name]
                .SingleOrDefault(x => x.Signature.Parameters.Select(x => x.Type).SequenceEqual(arguments))
                ;

        }

    }

}
