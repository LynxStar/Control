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

        public void Compile(Application app)
        {

            Clean();

            File.Copy("./CSharpProjectTemplate.xml", $"{CompileDir}/{app.Name}.csproj");

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

            var aspects = app
                .Types
                .Where(x => x.Key != "var")
                .Where(x => x.Key != "Program")
                .Select(x => x.Value)
                .Where(x => x.Source.From == "User Defined")
                .Select(x => x.BackingType)
                .Select(BuildType)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t{y}")
                ;

            var entryPoint = (app
                .Types
                ["Program"]
                .BackingType as Library)
                .Methods
                .Single(x => x.Key == "Main")
                ;

            var statements = entryPoint
                .Value
                .Single()
                .Block
                .Statements
                .Select(BuildStatementSource)
                .AggregateSafe(string.Empty, (x, y) => $"{x}\r\n\t\t\t{y}")
                ;

            var program = @$"
using System;
using System.Threading.Tasks;

namespace {app.Name}
{{    
    {aspects}
    public class Program
    {{

        static async Task Main(string[] args)
        {{

            {statements}

        }}
    }}

}}
";

            return program;

        }

        public string BuildType(Domain.Type type)
        {
            return type switch 
            { 
                Data data => BuildDataSource(data),
                Library library => BuildLibrarySource(library),
                Service service => BuildServiceSource(service)
            };

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

            var initializer = field.Type.BackingType.Name == "string"
                ? "String.Empty"
                : $"new {field.Type.BackingType.Name}()"
                ;

            return $"public {field.Type.BackingType.Name} {field.Identifier} {{ get; set; }} = {initializer};";
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

        public string BuildMethodSource(Method method, bool libraryMethod)
        {

            var parameters = method
                .Signature
                .Parameters
                .Select(x => $"{x.Type.BackingType.Name} {x.Identifier}")
                .AggregateSafe(string.Empty, (x, y) => $"{x}, {y}")
                ;

            var statements = method
                .Block
                .Statements
                .Select(BuildStatementSource)
                .AggregateSafe(string.Empty, (x,y) => $"{x}\r\n\t\t\t{y}")
                ;

            var libraryModifier = libraryMethod
                ? " static"
                : string.Empty
                ;

            return @$"
        public{libraryModifier} {method.Signature.Type.BackingType.Name} {method.Signature.Identifier}({parameters})
        {{

            {statements}

        }}
";
        }

        public string BuildStatementSource(Statement statement)
        {

            var source = statement switch
            {
                Declaration declaration => BuildDeclarationSource(declaration),
                Assignment assignment => BuildAssignmentSource(assignment),
                ReturnExpression returnExpression => BuildReturnExpressionSource(returnExpression),
                Expression expression => BuildExpressionSource(expression)
            };

            return $"{source};";

        }

        public string BuildDeclarationSource(Declaration declaration)
        {

            var typeName = declaration.TypeDefinition.Type.Name == "var"
                ? "var"
                : declaration.TypeDefinition.Type.BackingType.Name
                ;

            var decSource = $"{typeName} {declaration.TypeDefinition.Identifier}";

            decSource = declaration.InitializerExpression is null
                ? decSource
                : $"{decSource} = {BuildExpressionSource(declaration.InitializerExpression)}"
                ;

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
            return BuildUnaryExpressionSource(expression.UnaryExpression);
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
                ParensExpression expression => BuildExpressionSource(expression.Expression),
                NewExpression newExpression => BuildNewExpressionSource(newExpression),
                Invocation invocation => BuildInvocationSource(invocation)
            };
        }

        public string BuildLiteralSource(Literal literal)
        {

            object value = literal switch
            {
                Literal<int> i => i.Value,
                Literal<bool> b => b.Value,
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

        public string BuildServiceSource(Service service)
        {
            return "";
        }


    }
}
