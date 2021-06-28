using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Grammar
{

    public class RuleOption
    {
        public List<Clause> Clauses { get; set; } = new List<Clause>();

        public override string ToString()
        {
            return Clauses
                .Select(x => x.ToString())
                .Aggregate((x,y) => $"{x} {y}")
                ;
        }

    }
    
}
