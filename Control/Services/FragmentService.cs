using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Control.Services
{
    
    public class FragmentService
    {

        public IEnumerable<TokenRegex> BuildTokenRegex(IEnumerable<GrammarRule> rules)
        {
            var tokenRegexes = new List<TokenRegex>();

            var fragmentRules = rules
                .Where(x => x.RuleType == RuleType.Fragment)
                .ToDictionary(x => x.Name)
                ;

            var lexerRules = rules
                .Where(x => x.RuleType == RuleType.Token)
                ;

            foreach (var lexerRule in lexerRules)
            {
                var regex = FlattenRule(lexerRule, fragmentRules);

                var tokenRegex = new TokenRegex
                {
                    Name = lexerRule.Name,
                    Regex = new Regex(regex)
                };

                tokenRegexes.Add(tokenRegex);

                Console.WriteLine(tokenRegex);

            }


            return tokenRegexes;

        }

        public string FlattenRule(GrammarRule rule, Dictionary<string, GrammarRule> rules)
        {

            var alternatives = rule
                .Alternatives
                .Select(x => FlattenAlternative(x, rules))
                .ToList()
                ;

            var regex = alternatives
                .Aggregate((x, y) => $"({x})|({y})")
                ;

            return regex;

        }

        public string FlattenAlternative(Alternative alternative, Dictionary<string, GrammarRule> rules)
        {

            var clauses = alternative
                .RuleClauses
                .Select(x => FlattenRuleClause(x, rules))
                .ToList()
                ;

            var regex = clauses
                .Aggregate((x, y) => $"{x}{y}")
                ;

            return regex;

        }

        public string FlattenRuleClause(RuleClause ruleClause, Dictionary<string, GrammarRule> rules)
        {

            var clause = ruleClause.Clause;

            if (ruleClause.IsLiteral)
            {

                return Regex.Escape(ruleClause.Clause);

            }
            else
            {
                return FlattenFragment(ruleClause.Clause, rules);
            }

        }

        public string FlattenFragment(string clause, Dictionary<string, GrammarRule> rules)
        {

            if (rules.ContainsKey(clause))
            {

                var fragmentRule = rules[clause];

                var fragment = FlattenRule(fragmentRule, rules);

                return fragment;

            }

            return clause;

        }

    }
}
