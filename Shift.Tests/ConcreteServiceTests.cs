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
            return concreteService.MapTo<T>(node);

        }

        [TestMethod]
        public void EmptyLibraryTest()
        {

            var source = "data datastruct {}";

            var data = ConcreteMapper<Data>(source, "data");

            data.Identifier.IDENTIFIER.Should().Be("datastruct");
            data.Fields.Count().Should().Be(0);

        }

    }
}
