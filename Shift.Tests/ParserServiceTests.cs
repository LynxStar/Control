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

        public SyntaxNode ASTTester(string source, string entryFormKey)
        {

            var rules = grammarService.ConvertToBakedRules(ShiftGrammar.FullGrammar);
            var tokenStream = grammarService.ConvertToTokenStream(source, rules);

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

            node.UnaryToStart.Literal["INTEGER"].Capture.Should().Be("7");


        }


        [TestMethod]
        public void ReturnStatementTest()
        {

            var source = "return 7";

            var node = ASTTester(source, "return_expression");

            node.BasicLiteral["INTEGER"].Capture.Should().Be("7");


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

            node.Block.CGR.First().Statement.ReturnExpression.BasicLiteral["INTEGER"].Capture.Should().Be("7");


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
                .BasicLiteral
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

            declaration.CGR.First().BasicLiteral["INTEGER"].Capture.Should().Be("0");

        }

        [TestMethod]
        public void NewExpressionTest()
        {

            var source = "new penguin(6, 7, 8)";

            var node = ASTTester(source, "new_expression");

            node.Identifier.TokenValue.Should().Be("penguin");

            var expressions = node
                .Invocation
                .InvocationArgExpressions
                ;

            expressions[0].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("6");
            expressions[1].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("7");
            expressions[2].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("8");

        }

        [TestMethod]
        public void InvocationTest()
        {

            var source = "(7)";

            var node = ASTTester(source, "invocation");

            var expressions = node
                .InvocationArgExpressions
                ;

            expressions[0].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("7");

        }

        [TestMethod]
        public void DeclarationWithInitializerInvocationCallTest()
        {

            var source = "var beeds = new datastruct(7)";

            var node = ASTTester(source, "statement");

            var declaration = node
                .Declaration
                ;

            declaration.Type.TokenValue.Should().Be("var");
            declaration.Identifier.TokenValue.Should().Be("beeds");

            var newExpression = declaration.CGR.First().Expression.UnaryToStart.NewExpression;

            newExpression.Identifier.TokenValue.Should().Be("datastruct");

            newExpression.Invocation.InvocationArgExpressions[0].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("7");

        }

        [TestMethod]
        public void ExpressionChainIdentifierTest()
        {

            var source = "d.a";

            var node = ASTTester(source, "main_expression");

            var chains = node.Chain;

            node.ExpressionStart.Identifier.TokenValue.Should().Be("d");
            chains[0].MemberAccess.Identifier.TokenValue.Should().Be("a");

        }

        [TestMethod]
        public void AssignmentWithInvocationTest()
        {

            var source = "g.d.e = bob(7)";

            var node = ASTTester(source, "statement");

            var assignment = node
                .Assignment
                ;

            var accessor = assignment
                .Accessor
                ;

            accessor[0].Should().Be("g");
            accessor[1].Should().Be("d");
            accessor[2].Should().Be("e");

            var mainExpression = assignment
                .Expression
                .UnaryExpression
                .MainExpression
                ;

            mainExpression.ExpressionStart.Identifier.TokenValue.Should().Be("bob");
            mainExpression.Chain[0].Invocation.InvocationArgExpressions[0].UnaryToStart.Literal["INTEGER"].Capture.Should().Be("7");


        }

        [TestMethod]
        public void SidBobParenExpressionTest()
        {

            var source = "(sid).bob()";

            var node = ASTTester(source, "statement");

            var mainExpression = node
                .Expression
                .UnaryExpression
                .MainExpression
                ;

            mainExpression.ExpressionStart.ParenthesesExpression.UnaryToStart.Identifier.TokenValue.Should().Be("sid");

            mainExpression.Chain[0].MemberAccess.Identifier.TokenValue.Should().Be("bob");
            var foo = mainExpression.CGR.Count().Should().Be(2);

        }

    }

}
