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

        public SyntaxNode ConvertToAST(string grammar, string source, string entryFormKey)
        {

            var grammarRulesService = new RulesService();

            var rules = grammarRulesService.BuildRules(grammar);

            var fragmentService = new FragmentService();

            var bakedRules = fragmentService.BakeTokens(rules);

            var lexerService = new LexerService();

            var tokenStream = lexerService.Tokenize(source, bakedRules);

            var parserService = new ParserService();

            var entryRule = rules[entryFormKey];

            var ast = parserService.ParseTokenStream(tokenStream, entryRule);

            return ast;

        }

    }
}
