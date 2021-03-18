using Control.Grammar;
using Control.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Tests
{

    [TestClass]
    public class ParserServiceTests
    {

        GrammarService grammarService = new GrammarService();
        ParserService parserService = new ParserService();
        LinkedList<Token> tokenStream;

        public SyntaxNode ASTTester(string source, string entryFormKey)
        {

            var rules = grammarService.ConvertToBakedRules(ShiftGrammar.FullGrammar);
            tokenStream = grammarService.ConvertToTokenStream(source, rules);

            var entryRule = rules[entryFormKey];

            return parserService.ParseTokenStream(tokenStream, entryRule);

        }

        [TestMethod]
        public void EmptyLibraryTest()
        {

            var source = "library testLibrary {}";

            var node = ASTTester(source, "library");

            node.SyntaxNodes.Count().Should().Be(5);
            node.SyntaxNodes.Any(x => x.Rule.Name == "LIBRARY").Should().BeTrue();
            node.Identifier.TokenValue.Should().Be("testLibrary");

        }

        [TestMethod]
        public void EmptyMethodSignatureTest()
        {

            var source = "int foo()";

            var node = ASTTester(source, "signature");

            node.TypeDef.Type.TokenValue.Should().Be("int");
            node.TypeDef.Identifier.TokenValue.Should().Be("foo");

        }


        [TestMethod]
        public void LiteralExpressionTest()
        {

            var source = "7";

            var node = ASTTester(source, "expression");

            node["unary_expression"]["primary_expression"]["literal"]["INTEGER"].Capture.Should().Be("7");


        }


        [TestMethod]
        public void ReturnStatementTest()
        {

            var source = "return 7";

            var node = ASTTester(source, "return_expression");

            node["expression"]["unary_expression"]["primary_expression"]["literal"]["INTEGER"].Capture.Should().Be("7");


        }

        [TestMethod]
        public void LiteralMethodTest()
        {

            var source = @"
    int foo()
    {
        return 7;
    }
";

            var node = ASTTester(source, "method");

            node.Signature.TypeDef.Type.TokenValue.Should().Be("int");
            node.Signature.TypeDef.Identifier.TokenValue.Should().Be("foo");

            node.Block.CGR.First().Statement.ReturnExpression.Expression.UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("7");


        }

        [TestMethod]
        public void StringLiteralMethodTest()
        {

            var source = @"
    string bar(string a)
    {
        return ""adsfadf"";
    }
";

            var node = ASTTester(source, "method");

            node.Signature.TypeDef.Type.TokenValue.Should().Be("string");
            node.Signature.TypeDef.Identifier.TokenValue.Should().Be("bar");

            node
                .Block
                .CGR
                .First()
                .Statement
                .ReturnExpression
                .Expression
                .UnaryExpression
                .PrimaryExpression
                .Literal
                ["string"]
                ["STRING"]
                .Capture
                .Should().Be("\"adsfadf\"");


        }

        [TestMethod]
        public void MethodMultiParameterTest()
        {

            var source = @"
    int buck(string a, int b, bool c)
    {
        return 3;
    }
";

            var node = ASTTester(source, "method");

            node.Signature.TypeDef.Type.TokenValue.Should().Be("int");
            node.Signature.TypeDef.Identifier.TokenValue.Should().Be("buck");

            var parameters = node.Signature.CGR.First();

            var firstParameter = parameters.Parameter;

            firstParameter.TypeDef.Type.TokenValue.Should().Be("string");
            firstParameter.TypeDef.Identifier.TokenValue.Should().Be("a");

            parameters.CGR.Count().Should().Be(2);

            parameters.CGR[0].Parameter.TypeDef.Type.TokenValue.Should().Be("int");
            parameters.CGR[0].Parameter.TypeDef.Identifier.TokenValue.Should().Be("b");

            parameters.CGR[1].Parameter.TypeDef.Type.TokenValue.Should().Be("bool");
            parameters.CGR[1].Parameter.TypeDef.Identifier.TokenValue.Should().Be("c");


        }

        [TestMethod]
        public void BasicLibraryTest()
        {

            var source = @"
library blue
{
    int foo()
    {
        return 7;
    }

    string bar(string a)
    {
        return ""adsfadf"";
    }

    string baz(string a)
    {
        return ""another string"";
    }

    int buck(string a, int b)
    {
        return 3;
    }

}
";

            var node = ASTTester(source, "library");

            node.Identifier.TokenValue.Should().Be("blue");

            var methods = node.CGR.Select(x => x.Method);

            methods.Count().Should().Be(4);

        }

        [TestMethod]
        public void DeclarationStatementTest()
        {

            var source = "int f";

            var node = ASTTester(source, "statement");

            var declaration = node
                .Declaration
                ;

            declaration.Type.TokenValue.Should().Be("int");
            declaration.Identifier.TokenValue.Should().Be("f");

        }

        [TestMethod]
        public void DeclarationWithInitializerTest()
        {

            var source = "int g = 0";

            var node = ASTTester(source, "statement");

            var declaration = node
                .Declaration
                ;

            declaration.Type.TokenValue.Should().Be("int");
            declaration.Identifier.TokenValue.Should().Be("g");

            declaration.CGR.First().Expression.UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("0");

        }

        [TestMethod]
        public void NewExpressionTest()
        {

            var source = "new penguin(6, 7, 8)";

            var node = ASTTester(source, "new_expression");

            node.Identifier.TokenValue.Should().Be("penguin");

            var expressions = node
                .Invocation
                .ArgumentExpressions
                ;

            expressions[0].UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("6");
            expressions[1].UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("7");
            expressions[2].UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("8");

        }

        [TestMethod]
        public void InvocationTest()
        {

            var source = "(7)";

            //var beeds = new<datastruct>(7);
            //new datastruct(7);

            var node = ASTTester(source, "invocation");

            var expressions = node
                .ArgumentExpressions
                ;

            expressions[0].UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("7");

        }


        [TestMethod]
        public void DeclarationWithInitializerInvocationCallTest()
        {

            var source = "var beeds = new datastruct(7)";

            //var beeds = new<datastruct>(7);
            //new datastruct(7);

            var node = ASTTester(source, "statement");

            var declaration = node
                .Declaration
                ;

            declaration.Type.TokenValue.Should().Be("var");
            declaration.Identifier.TokenValue.Should().Be("beeds");

            var newExpression = declaration.CGR.First().Expression.UnaryExpression.PrimaryExpression.NewExpression;

            newExpression.Identifier.TokenValue.Should().Be("datastruct");

            newExpression.Invocation.ArgumentExpressions[0].UnaryExpression.PrimaryExpression.Literal["INTEGER"].Capture.Should().Be("7");

        }

    }

}
