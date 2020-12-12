using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Grammar
{
    public class RuleNode
    {


        public GrammarRule Rule { get; set; }

        public Token Token { get; set; }

        public List<RuleNode> Nodes { get; set; } = new List<RuleNode>();

    }
}
