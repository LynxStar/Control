using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{

    public class GrammarService
    {

        public Dictionary<string, Rule> ConvertToBakedRules(string grammar)
        {

            var grammarRulesService = new RulesService();

            var rules = grammarRulesService.BuildRules(grammar);

            var fragmentService = new FragmentService();

            var bakedRules = fragmentService.BakeTokens(rules);

            return bakedRules;

        }

        public LinkedList<Token> ConvertToTokenStream(string source, Dictionary<string, Rule> rules, bool dumpStream = false)
        {

            var lexerService = new LexerService();

            var tokenStream = lexerService.Tokenize(source, rules, dumpStream);

            return tokenStream;

        }

        public SyntaxNode ConvertToAST(string grammar, string source, string entryFormKey)
        {

            var rules = ConvertToBakedRules(grammar);

            var tokenStream = ConvertToTokenStream(source, rules, true);

            var parserService = new ParserService();

            var entryRule = rules[entryFormKey];

            var ast = parserService.ParseTokenStream(tokenStream, entryRule);

            return ast;

        }

        public T ConvertTo<T>(string grammar, string source, string entryFormKey) where T : new()
        {

            var ast = ConvertToAST(grammar, source, entryFormKey);

            ConcreteService concreteService = new ConcreteService();

            var value = concreteService.MapNodeToObject<T>(ast);

            return value;

        }

    }
}
