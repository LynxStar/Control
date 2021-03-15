using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{

    public class Rule
    {
        public RuleType RuleType { get; set; }
        public string Name { get; set; }
        public string Regex { get; set; }
        public List<RuleOption> Options { get; set; } = new List<RuleOption>();

        public override string ToString()
        {

            var output = String.IsNullOrEmpty(Regex)
                ? Options.Select(x => x.ToString()).Aggregate((x,y) => $"{x} | {y}")
                : Regex
                ;


            return $"{RuleType} {Name}: {output}";
        }

    }
}
