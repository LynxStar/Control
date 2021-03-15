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


    }

}
