using Shift.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Services
{
    
    public class CompilerService
    {

        private readonly string CompileDir = "./compile";
        private readonly ExpressionBuilder expressionBuilder = new ExpressionBuilder();

        public void Compile(Application app)
        {

            Clean();

            File.Copy("./CSharpProjectTemplate.xml", $"{CompileDir}/{app.Name}.csproj");

            Directory.CreateDirectory($"{CompileDir}/Data");
            Directory.CreateDirectory($"{CompileDir}/Libraries");
            Directory.CreateDirectory($"{CompileDir}/Services");

            Transpile(app);

            Compile();

        }

        public void Transpile(Application app)
        {
            var transpile = BuildAppSource(app);
            File.WriteAllText($"{CompileDir}/Program.cs", transpile);

        }

        public void Compile()
        {
            Process dotnetProcess = new Process();

            dotnetProcess.StartInfo.FileName = "dotnet";
            dotnetProcess.StartInfo.Arguments = "build";
            dotnetProcess.StartInfo.RedirectStandardInput = true;
            dotnetProcess.StartInfo.RedirectStandardOutput = true;
            dotnetProcess.StartInfo.CreateNoWindow = true;
            dotnetProcess.StartInfo.UseShellExecute = false;
            dotnetProcess.StartInfo.WorkingDirectory = $"{Directory.GetCurrentDirectory()}\\compile";

            dotnetProcess.EnableRaisingEvents = true;

            dotnetProcess.OutputDataReceived += new DataReceivedEventHandler
            (
                delegate (object sender, DataReceivedEventArgs e)
                {
                    // append the new data to the data already read-in
                    Console.WriteLine(e.Data);
                }
            );

            dotnetProcess.Start();
            dotnetProcess.BeginOutputReadLine();

            dotnetProcess.WaitForExit();
            dotnetProcess.CancelOutputRead();

            if(dotnetProcess.ExitCode == 1)
            {
                Console.ReadLine();
            }

        }

        public void Clean()
        {

            if(!Directory.Exists(CompileDir))
            {
                Directory.CreateDirectory(CompileDir);
            }

            var di = new DirectoryInfo(CompileDir);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public string BuildAppSource(Application app)
        {

            var unresolvedTypes = app
                .Tracker
                .ConsumedTypes
                .Where(x => x.Key != "var")
                .Where(x => x.Value.Source == TypeSource.Unknown)
                ;

            if(unresolvedTypes.Any())
            {
                throw new Exception("You failed to link everything wtf?");
            }

            app
                .Types
                .Where(x => x.Key != "var")
                .Where(x => x.Key != "Program")
                .Select(x => x.Value)
                .ToList()
                .ForEach(TranspileType)
                ;

            var entryPoint = (app
                .Types
                ["Program"]
                as Library)
                .Methods
                .Single(x => x.Key == "Main")
                ;

            var statements = entryPoint
                .Value
                .Single()
                .Block
                .Statements
                .Select(BuildStatementSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n{y}")
                ;

            var program = @$"
using System;
using System.Threading.Tasks;
using Shift.Language;
using {app.Name};

{statements}
";

            return program;

        }

        public void TranspileType(Domain.Type type)
        {

            var location = type switch
            {
                Data data => "Data",
                Library library => "Libraries",
                Service service => "Services"
            };

            var source = type switch 
            { 
                Data data => BuildDataSource(data),
                Library library => BuildLibrarySource(library),
                Service service => BuildServiceSource(service)
            };

            source = @$"
using System;
using System.Threading.Tasks;
using Shift.Language;

namespace {type.Namespace}
{{    
    {source}
}}
";

            File.WriteAllText($"{CompileDir}/{location}/{type.Name}.cs", source);

        }

        public string BuildDataSource(Data data)
        {

            var fields = data
                .Fields
                .Values
                .Select(BuildFieldSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t{y}")
                ;

            return @$"
    public class {data.Name}
    {{

        {fields}

    }}
            ";

        }

        public string BuildFieldSource(Field field)
        {

            var initializer = expressionBuilder
                .TypeDefaultExpression(field.Type.BackingType)
                ;

            var initializerSource = BuildExpressionSource(initializer);

            return $"public {field.Type.BackingType.Name} {field.Identifier} {{ get; set; }} = {initializerSource};";
        }

        public string BuildLibrarySource(Library library)
        {

            var methods = library
                .Methods
                .SelectMany(x => x.Value)
                .Select(x => BuildMethodSource(x, true))
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t{y}")
                ;

            return @$"
    public static class {library.Name}
    {{

        {methods}

    }}
            ";
        }

        public string BuildServiceSource(Service service)
        {
            var methods = service
                .Methods
                .SelectMany(x => x.Value)
                .Select(x => BuildMethodSource(x, false))
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t{y}")
                ;

            var fields = service
                .Fields
                .Values
                .Select(BuildFieldSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t{y}")
                ;

            var constructors = BuildConstructorsSource(service.Constructors, service.Name);

            return @$"
    public class {service.Name}
    {{

        {fields}

        {constructors}

        {methods}

    }}
            ";
        }

        public string BuildConstructorsSource(IEnumerable<Method> constructors, string serviceName)
        {
            //

            var containsDefault = constructors
                .Any(x => x.Signature.Parameters.Count() == 0)
                ;

            var builtSource = constructors
                .Select(x => BuildConstructorSource(x, serviceName))
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t{y}")
                ;

            if(!containsDefault)
            {
                builtSource = $"public {serviceName}() {{  }}\r\n\r\n{builtSource}";
            }

            return builtSource;

        }

        public string BuildConstructorSource(Method method, string serviceName)
        {
            var parameters = BuildParameterSource(method);
            string block = BuildBlockSource(method.Block);

            return @$"
        public {serviceName}({parameters}){block}
";
        }

        private string BuildBlockSource(Block block)
        {

            var statements = block
                .Statements
                .Select(BuildStatementSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t\t{y}")
                ;

            return $@"
        {{

            {statements}

        }}

";
        }

        public string BuildMethodSource(Method method, bool libraryMethod)
        {
            string parameters = BuildParameterSource(method);
            string block = BuildBlockSource(method.Block);

            var libraryModifier = libraryMethod
                ? " static"
                : string.Empty
                ;

            return @$"
        public{libraryModifier} {method.Signature.Type.BackingType.Name} {method.Signature.Identifier}({parameters}){block}
";
        }

        private static string BuildParameterSource(Method method)
        {
            return method
                .Signature
                .Parameters
                .Select(x => $"{x.Type.BackingType.Name} {x.Identifier}")
                .AggregateSafe(string.Empty, (x, y) => $"{x}, {y}");
        }

        public string BuildStatementSource(Statement statement)
        {

            var source = statement switch
            {
                ControlStatement control => BuildControlSource(control),
                Declaration declaration => BuildDeclarationSource(declaration),
                Assignment assignment => BuildAssignmentSource(assignment),
                ReturnExpression returnExpression => BuildReturnExpressionSource(returnExpression),
                Expression expression => BuildExpressionSource(expression)
            };

            return $"{source};";

        }

        public string BuildControlSource(ControlStatement control)
        {

            var name = control switch 
            { 
                IfControl => "if",
                WhileControl => "while"
            };

            var condition = BuildConditionalOrExpressionSource(control.Condition);
            var block = BuildBlockSource(control.Block);

            return $"{name}({condition})\r\n\t\t\t{{{block}\t\t\t}}\r\n";

        }

        public string BuildDeclarationSource(Declaration declaration)
        {

            var typeName = declaration.TypeDefinition.Type.Name == "var"
                ? "var"
                : declaration.TypeDefinition.Type.BackingType.Name
                ;

            var decSource = $"{typeName} {declaration.TypeDefinition.Identifier}";

            var initializer = declaration.InitializerExpression is not null
                ? declaration.InitializerExpression
                : expressionBuilder.TypeDefaultExpression(declaration.TypeDefinition.Type.BackingType)
                ;

            decSource = $"{decSource} = {BuildExpressionSource(initializer)}";

            return decSource;

        }

        public string BuildAssignmentSource(Assignment assignment)
        {

            var identifier = assignment
                .IdentifierChain
                .Select(BuildIdentifierSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}.{y}")
                ;

            var expression = BuildExpressionSource(assignment.Expression);

            return $"{identifier} = {expression}";

        }

        public string BuildReturnExpressionSource(ReturnExpression returnExpression)
        {

            var expression = BuildExpressionSource(returnExpression.Expression);

            return $"return {expression}";

        }

        public string BuildExpressionSource(Expression expression)
        {

            return expression switch
            {
                ConditionalOrExpression conditionalOrExpression => BuildConditionalOrExpressionSource(conditionalOrExpression),
            };
        }

        public string BuildConditionalOrExpressionSource(ConditionalOrExpression expr)
        {

            var left = BuildConditionalAndExpressionSource(expr.ConditionalAndExpression);
            var chain = expr
                .ConditionalAndExpressions
                .Select(BuildConditionalAndExpressionSource)
                .Aggregate(left, (x,y) => $"{x} OR {y}")
                ;

            return chain;

        }

        public string BuildConditionalAndExpressionSource(ConditionalAndExpression expr)
        {
            var left = BuildEqualityExpressionSource(expr.EqualityExpression);
            var chain = expr
                .EqualityExpressions
                .Select(BuildEqualityExpressionSource)
                .Aggregate(left, (x, y) => $"{x} AND {y}")
                ;

            return chain;
        }

        public string BuildEqualityExpressionSource(EqualityExpression expr)
        {

            var left = BuildRelationalExpressionSource(expr.Left);

            if(expr.Right is null)
            {
                return left;
            }

            var op = expr.EqualityOperator switch
            {
                EqualityOperator.Equals => "==",
                EqualityOperator.NotEquals => "!="
            };

            var right = BuildRelationalExpressionSource(expr.Right);

            return $"{left} {op} {right}";
        }

        public string BuildRelationalExpressionSource(RelationalExpression expr)
        {
            var left = BuildAdditiveExpressionSource(expr.Left);

            if (expr.Right is null)
            {
                return left;
            }

            var op = expr.RelationalOperator switch
            {
                RelationalOperator.GreaterThan => ">",
                RelationalOperator.GreaterThanOrEqual => ">=",
                RelationalOperator.LessThan => "<",
                RelationalOperator.LessThanOrEqual => "<=",
            };

            var right = BuildAdditiveExpressionSource(expr.Right);

            return $"{left} {op} {right}";
        }

        public string BuildAdditiveExpressionSource(AdditiveExpression expr)
        {
            var left = BuildMultiplicativeExpressionSource(expr.MultiplicativeExpression);
            var chain = expr
                .MultiplicativeExpressions
                .Select(x => (BuildAdditiveOperatorSource(x.op), BuildMultiplicativeExpressionSource(x.expr)))
                .Aggregate(left, (x, y) => $"{x} {y.Item1} {y.Item2}")
                ;

            return chain;
        }

        public string BuildAdditiveOperatorSource(AdditiveOperator op)
        {
            return op switch
            {
                AdditiveOperator.Addition => "+",
                AdditiveOperator.Subtraction => "-"
            };
        }

        public string BuildMultiplicativeExpressionSource(MultiplicativeExpression expr)
        {
            var left = BuildUnaryExpressionSource(expr.UnaryExpression);
            var chain = expr
                .UnaryExpressions
                .Select(x => (BuildMultiplicativeOperatorSource(x.op), BuildUnaryExpressionSource(x.expr)))
                .Aggregate(left, (x, y) => $"{x} {y.Item1} {y.Item2}")
                ;

            return chain;
        }

        public string BuildMultiplicativeOperatorSource(MultiplicativeOperator op)
        {
            return op switch
            {
                MultiplicativeOperator.Multiplication => "*",
                MultiplicativeOperator.Division => "/"
            };
        }

        public string BuildUnaryExpressionSource(UnaryExpression unaryExpression)
        {
            return BuildMainExpression(unaryExpression.MainExpression);
        }

        public string BuildMainExpression(MainExpression mainExpression)
        {

            var start = BuildExpressionStartSource(mainExpression.ExpressionStart);
            var chain = mainExpression
                .ExpressionChain
                .Select(BuildExpressionChain)
                .AggregateSafe(string.Empty, (x,y) => $"{x}{y}")
                ;

            return $"{start}{chain}";

        }

        public string BuildExpressionStartSource(ExpressionStart expressionStart)
        {
            return expressionStart switch
            {
                Literal literal => BuildLiteralSource(literal),
                Identifier identifier => BuildIdentifierSource(identifier),
                ParensExpression expression => $"({BuildExpressionSource(expression.Expression)})",
                NewExpression newExpression => BuildNewExpressionSource(newExpression),
                //Invocation invocation => BuildInvocationSource(invocation) //You never can have an invocation as the start that would be something like (), 
                //which will parse as a parents expression inside
            };
        }

        public string BuildLiteralSource(Literal literal)
        {

            object value = literal switch
            {
                Literal<int> i => i.Value,
                Literal<bool> b => b.Value.ToString().ToLower(),
                Literal<string> s => s.Value
            };

            return value
                .ToString()
                ;
        }

        public string BuildIdentifierSource(Identifier identifier)
        {
            return identifier.Path;
        }

        public string BuildNewExpressionSource(NewExpression newExpression)
        {

            var identifier = BuildIdentifierSource(newExpression.Identifier);
            var invocation = BuildInvocationSource(newExpression.Invocation);

            return $"new {identifier}{invocation}";

        }

        public string BuildInvocationSource(Invocation invocation)
        {

            var arguments = invocation
                .Arguments
                .Select(BuildExpressionSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}, {y}")
                ;

            return $"({arguments})";

        }

        public string BuildExpressionChain(ExpressionChain expressionChain)
        {

            return expressionChain switch
            {
                Identifier identifier => $".{BuildIdentifierSource(identifier)}",
                Invocation invocation => BuildInvocationSource(invocation)
            };

        }

    }
}
