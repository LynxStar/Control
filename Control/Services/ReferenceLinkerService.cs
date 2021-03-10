using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{
    
    public class ReferenceLinkerService
    {

        public Dictionary<string, Rule> LinkRules(Dictionary<string, Rule> rules)
        {

            var c = rules
                .SelectMany(x => x.Value.Options)
                .Select(ExtractOptionClauses)
                .SelectMany(x => x)
                .ToList()
                ;

            LinkClauses(c, rules);

            return rules;

        }

        public void LinkClauses(IEnumerable<Clause> clauses, Dictionary<string, Rule> rules)
        {
            //Link rules
            foreach (var clause in clauses)
            {
                clause.Reference = rules[clause.Value];
            }
        }

        public List<Clause> ExtractOptionClauses(RuleOption option)
        {

            var references = option
                .Clauses
                .Where(x => x.ClauseType == ClauseType.Reference)
                .ToList()
                ;

            var captureGroups = option
                .Clauses
                .Where(x => x.ClauseType == ClauseType.CaptureGroup)
                .Select(x => x.CaptureGroup)
                .Select(ExtractOptionClauses)
                .SelectMany(x => x)
                .Where(x => x.ClauseType == ClauseType.Reference)
                .ToList()
                ;

            var clauses = new List<Clause>();

            clauses.AddRange(references);
            clauses.AddRange(captureGroups);

            return clauses;

        }

    }

}
