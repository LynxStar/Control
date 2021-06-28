using Control.Grammar;
using Control.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shift.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Tests
{

    [TestClass]
    public class ConcreteServiceTests
    {

        GrammarService grammarService = new GrammarService();
        ParserService parserService = new ParserService();
        ConcreteService concreteService = new ConcreteService();

        public SyntaxNode ASTTester(string source, string entryFormKey)
        {

            var rules = grammarService.ConvertToBakedRules(ShiftGrammar.FullGrammar);
            var tokenStream = grammarService.ConvertToTokenStream(source, rules);

            var entryRule = rules[entryFormKey];

            return parserService.ParseTokenStream(tokenStream, entryRule);

        }

        public T ConcreteMapper<T>(string source, string entryFormKey) where T : new()
        {

            var node = ASTTester(source, entryFormKey);
            return concreteService.MapNodeToObject<T>(node);

        }

        public T ConcreteMapDirectly<T>(string source, string entryFormKey) where T : class
        {
            var node = ASTTester(source, entryFormKey);

            var context = new TargetContext
            {
                TargetNode = node,
                DestinationType = typeof(T),
            };

            return concreteService.MapDirectly(context) as T;

        }

        [TestMethod]
        public void EmptyDataTest()
        {

            var source = "data datastruct {}";

            var data = ConcreteMapper<Data>(source, "data");

            data.Identifier.IDENTIFIER.Should().Be("datastruct");
            data.Fields.Count().Should().Be(0);

        }

        [TestMethod]
        public void FullDataTest()
        {

            var source = @"
data datastruct
{
    
    string bob;
    string sid;

    int mark;//this shouldn't be an iussue

    bool sydney;
//or this
//int foo
}
";

            var data = ConcreteMapper<Data>(source, "data");

            data.Identifier.IDENTIFIER.Should().Be("datastruct");
            data.Fields.Count().Should().Be(4);

            data.Fields[2].TypeDef.Type.IDENTIFIER.Should().Be("int");

            data.Fields[3].TypeDef.Identifier.IDENTIFIER.Should().Be("sydney");

        }

        [TestMethod]
        public void EmptySignatureTest()
        {

            var source = "int bob()";

            var signature = ConcreteMapper<Signature>(source, "signature");

            signature.TypeDef.Type.IDENTIFIER.Should().Be("int");
            signature.TypeDef.Identifier.IDENTIFIER.Should().Be("bob");

            signature.Parameters.Should().BeNull();

        }

        [TestMethod]
        public void SignatureSignalParameterTest()
        {

            var source = "int bob(string a)";

            var signature = ConcreteMapper<Signature>(source, "signature");

            signature.TypeDef.Type.IDENTIFIER.Should().Be("int");
            signature.TypeDef.Identifier.IDENTIFIER.Should().Be("bob");

            signature.Parameters.Parameter.TypeDef.Type.IDENTIFIER.Should().Be("string");
            signature.Parameters.Parameter.TypeDef.Identifier.IDENTIFIER.Should().Be("a");

        }

        [TestMethod]
        public void SignatureTest()
        {

            var source = "int bob(bool f, string a, pig honk)";

            var signature = ConcreteMapper<Signature>(source, "signature");

            signature.TypeDef.Type.IDENTIFIER.Should().Be("int");
            signature.TypeDef.Identifier.IDENTIFIER.Should().Be("bob");

            signature.Parameters.Parameter.TypeDef.Type.IDENTIFIER.Should().Be("bool");
            signature.Parameters.Parameter.TypeDef.Identifier.IDENTIFIER.Should().Be("f");

            signature.Parameters.AdditionalParameters[1].TypeDef.Type.IDENTIFIER.Should().Be("pig");
            signature.Parameters.AdditionalParameters[1].TypeDef.Identifier.IDENTIFIER.Should().Be("honk");


        }

        [TestMethod]
        public void BlockTest()
        {

            var source = @"
{
    return 7;
}
";

            var block = ConcreteMapper<Block>(source, "block");

        }

        [TestMethod]
        public void RuleOptionsTest()
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

            var library = ConcreteMapper<Library>(source, "library");


        }

        [TestMethod]
        public void BooleanTrueTest()
        {

            var source = @"true";

            var boolean = ConcreteMapDirectly<Concrete.Boolean>(source, "boolean");

            boolean.Token.Should().Be("boolean");
            boolean.Value.Should().Be("true");

        }

        [TestMethod]
        public void BooleanFalseTest()
        {

            var source = @"false";

            var boolean = ConcreteMapDirectly<Concrete.Boolean>(source, "boolean");

            boolean.Token.Should().Be("boolean");
            boolean.Value.Should().Be("false");

        }

        [TestMethod]
        public void LiteralBooleanTest()
        {

            var source = @"false";

            var literal = ConcreteMapDirectly<Literal>(source, "literal");

            (literal.Value as Concrete.Boolean).Token.Should().Be("boolean");
            (literal.Value as Concrete.Boolean).Value.Should().Be("false");

        }

        [TestMethod]
        public void LiteralIntegerTest()
        {

            var source = @"7";

            var literal = ConcreteMapDirectly<Literal>(source, "literal");

            (literal.Value as Control.TokenValue).Token.Should().Be("INTEGER");
            (literal.Value as Control.TokenValue).Value.Should().Be("7");

        }

        [TestMethod]
        public void LiteralStringTest()
        {

            var source = @"""adfasdf""";

            var literal = ConcreteMapDirectly<Literal>(source, "literal");

            (literal.Value as Concrete.String).STRING.Should().Be(@"""adfasdf""");

        }

        [TestMethod]
        public void ExpressionStartIdentifierTest()
        {
            var source = "bigbobbully";

            var expression_start = ConcreteMapDirectly<ExpressionStart>(source, "expression_start");

            (expression_start.Value as Identifier).IDENTIFIER.Should().Be("bigbobbully");
        }

        [TestMethod]
        public void LiteralExpressionTest()
        {

            var source = "7";

            var expression = ConcreteMapDirectly<Expression>(source, "expression");

            var literal = (expression.Value as ConditionalOrExpression)
                .ConditionalAndExpression
                .EqualityExpression
                .RelationalExpression
                .AdditiveExpression
                .MultiplicativeExpression
                .UnaryExpression
                .MainExpression
                .ExpressionStart
                .Value as Literal
                ;

            (literal.Value as Control.TokenValue).Token.Should().Be("INTEGER");
            (literal.Value as Control.TokenValue).Value.Should().Be("7");

        }

        [TestMethod]
        public void ParensWithInnerLiteralExpressionTest()
        {

            var source = "(7)";

            var parenExpression = ConcreteMapDirectly<ParensExpression>(source, "parens_expression");
            var literal = (parenExpression.Expression.Value as ConditionalOrExpression)
                .ConditionalAndExpression
                .EqualityExpression
                .RelationalExpression
                .AdditiveExpression
                .MultiplicativeExpression
                .UnaryExpression
                .MainExpression
                .ExpressionStart
                .Value as Literal
                ;

            (literal.Value as Control.TokenValue).Token.Should().Be("INTEGER");
            (literal.Value as Control.TokenValue).Value.Should().Be("7");
        }

        [TestMethod]
        public void NestedParensExpression()
        {

            var source = "((7))";

            var parenExpression = ConcreteMapDirectly<ParensExpression>(source, "parens_expression");

            var innerParens = (parenExpression.Expression.Value as ConditionalOrExpression)
                .ConditionalAndExpression
                .EqualityExpression
                .RelationalExpression
                .AdditiveExpression
                .MultiplicativeExpression
                .UnaryExpression
                .MainExpression
                .ExpressionStart
                .Value as ParensExpression
                ;


            var literal = (innerParens.Expression.Value as ConditionalOrExpression)
                .ConditionalAndExpression
                .EqualityExpression
                .RelationalExpression
                .AdditiveExpression
                .MultiplicativeExpression
                .UnaryExpression
                .MainExpression
                .ExpressionStart
                .Value as Literal
                ;

            (literal.Value as Control.TokenValue).Token.Should().Be("INTEGER");
            (literal.Value as Control.TokenValue).Value.Should().Be("7");

        }

        [TestMethod]
        public void ExpressionStartLiteralTest()
        {

            var source = "7";

            var expression_start = ConcreteMapDirectly<ExpressionStart>(source, "expression_start");

            ((expression_start.Value as Literal).Value as Control.TokenValue).Token.Should().Be("INTEGER");
            ((expression_start.Value as Literal).Value as Control.TokenValue).Value.Should().Be("7");

        }

        [TestMethod]
        public void RelationalTest()
        {
            var source = "3 < 4";

            var relationalExpression = ConcreteMapDirectly<RelationalExpression>(source, "relational_expression");
        }

        [TestMethod]
        public void StatementMultiplicativeInsanity()
        {
            
            var source = "int f;";

            var statement = ConcreteMapDirectly<Statement>(source, "statement");

        }

    }
}
