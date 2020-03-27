using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Control.Grammar
{
    public class GrammarRule
    {

        public RuleType RuleType { get; set; }
        public string Name { get; set; }
        public List<Alternative> Alternatives { get; set; }

        public override string ToString()
        {

            var alternatives = Alternatives.Select(x => x.ToString()).Aggregate((x, y) => $"{x} | {y}");

            return $"{RuleType} {Name} : {alternatives}";
        }

    }

    public enum RuleType
    {
        Fragment,
        Token,
        Rule,
        Noop
    }

}
