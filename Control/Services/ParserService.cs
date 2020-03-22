using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Control.Services
{
    
    public class ParserService
    {

        private readonly FragmentService _fragmentService = new FragmentService();
        private readonly LexerService _lexer = new LexerService();

        public List<Token> Parse(string source, IEnumerable<GrammarRule> rules)
        {

            //Stage 1: DeFragment: Applies the fragments to the lexer tokens

            var tokenRules = _fragmentService.BuildTokenRegex(rules);

            //Stage 2: Lexer

            _lexer.Tokenize(source, tokenRules);

            //Stage 3: Parser

            return null;

        }

        

    }
}
