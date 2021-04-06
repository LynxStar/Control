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

    }
}
