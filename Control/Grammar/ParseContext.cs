using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Grammar
{
    public class ParseContext
    {

        public Dictionary<string, GrammarRule> SourceRules { get; set; }
        public IEnumerable<TokenRegex> TokenRules { get; set; }
        public LinkedListNode<Token> CurrentNode { get; set; }

    }

}
