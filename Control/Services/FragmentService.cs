using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Control.Services
{
    
    public class FragmentService
    {

        public Dictionary<string, Rule> BakeTokens(Dictionary<string, Rule> rules)
        {


            var bakedRules = BakeRules(rules);

            return bakedRules
                .Where(x => x.Value.RuleType != RuleType.Fragment)
                .ToDictionary(x => x.Key, x => x.Value)
                ;

        }
        
        public Dictionary<string, Rule> BakeRules(Dictionary<string, Rule> rules)
        {

            foreach(var rule in rules)
            {

                if(rule.Value.RuleType == RuleType.Form)
                {
                    continue;
                }

                rule.Value.Regex = BuildRuleRegex(rule.Value, rules);
            }

            return rules;

        }

        public string BuildRuleRegex(Rule rule, Dictionary<string, Rule> rules)
        {

            if(!String.IsNullOrWhiteSpace(rule.Regex))
            {
                return rule.Regex;
            }

            var optionRegexes = rule
                .Options
                .Select(x => BuildOptionRegex(x, rules))
                ;

            var regex = optionRegexes
                .Aggregate((x, y) => $"({x})|({y})")
                ;

            return regex;
        }

        public string BuildOptionRegex(RuleOption option, Dictionary<string, Rule> rules)
        {

            var optionRegexes = option
                .Clauses
                .Select(x => BuildClauseRegex(x, rules))
                ;

            var regex = optionRegexes
                .Aggregate((x, y) => $"{x}{y}")
                ;

            return regex;

        }

        public string BuildClauseRegex(Clause clause, Dictionary<string, Rule> rules)
        {

            if (clause.ClauseType == ClauseType.CaptureGroup)
            {

                var regex = BuildOptionRegex(clause.CaptureGroup, rules);
                var modifier = BuildModifierRegex(clause.CaptureGroup.Modifier);

                return $"({regex}){modifier}";

            }
            else if(clause.ClauseType == ClauseType.Reference)
            {

                var reference = rules[clause.Value];

                if(reference.RuleType == RuleType.Form)
                {
                    throw new Exception("You need more unit tests. WTF Happened?");
                }

                return BuildRuleRegex(reference, rules);

            }
            else if (clause.ClauseType == ClauseType.Literal)
            {
                return Regex.Escape(clause.Value);
            }
            else if(clause.ClauseType == ClauseType.Regex)
            {
                return clause.Value;
            }
            else
            {
                throw new Exception("You forgot to program this");
            }

        }

        public string BuildModifierRegex(CaptureModifier modifier)
        {

            return modifier switch
            {

                CaptureModifier.None => String.Empty,
                CaptureModifier.NoneToOne => "?",
                CaptureModifier.OneOrMore => "+",
                CaptureModifier.Optional => "*"

            };

        }

    }
}
