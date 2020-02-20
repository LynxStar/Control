using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Control.Grammar
{
    public class Alternative
    {
        public List<RuleClause> RuleClauses { get; set; } = new List<RuleClause>();

        public override string ToString()
        {
            return RuleClauses.Select(x => x.ToString()).Aggregate((x, y) => $"{x} {y}");
        }

    }
}
