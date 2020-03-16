using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Control.Services
{
    public class RulesService
    {

        public List<GrammarRule> BuildGrammarRules(string grammarText)
        {

            var grammarRules = new List<GrammarRule>();

            var rules = grammarText
                .Replace("';'", "&SEMICOLON")
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Replace("&SEMICOLON", "';'"))
                .Select(ParseGrammarRule)
                .ToList();

            grammarRules.AddRange(rules);

            return grammarRules;

        }

        public GrammarRule ParseGrammarRule(string ruleText)
        {

            var grammarRule = new GrammarRule();

            var type = ruleText.Before(" ").TrimWhitespace();

            grammarRule.RuleType = type switch
            {
                "rule" => RuleType.Rule,
                "token" => RuleType.Token,
                "fragment" => RuleType.Fragment,
                _ => throw new Exception("herp fucking derp")
            };

            grammarRule.Name = ruleText.StripFrom(" ").Before(":").TrimWhitespace();
            var ruleClauses = ruleText.After(":").TrimWhitespace();

            grammarRule.Alternatives = ParseAlternatives(ruleClauses);

            Console.WriteLine(grammarRule);

            return grammarRule;

        }

        public List<Alternative> ParseAlternatives(string text)
        {

            return text
                .Replace("||", "&OR")
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Replace("&OR", "||"))
                .Select(x => new Alternative
                {
                    RuleClauses = x
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(y => ParseRuleClause(y))
                        .ToList()
                })
                .ToList();

        }

        public RuleClause ParseRuleClause(string text)
        {

            var clause = new RuleClause();

            if (text.StartsWith("'")
                && text.EndsWith("'")
                && text.Count(x => x == '\'') == 2)
            {

                clause.Clause = text;
                clause.Qualifier = ClauseQualifier.None;

                return clause;

            }

            var parts = text
                .Split('&', StringSplitOptions.None)
                .Select(x => x.TrimWhitespace());

            clause.Clause = parts.First().Trim('*', '?', '+');

            var qualifierText = parts.First().After(clause.Clause);

            clause.Qualifier = qualifierText switch
            {
                "*" => ClauseQualifier.Optional,
                "?" => ClauseQualifier.NoneToOne,
                "+" => ClauseQualifier.OneOrMore,
                _ => ClauseQualifier.None
            };

            if (parts.Count() > 1)
            {
                clause.DelimiterInUse = true;

                var second = parts.Skip(1).First();

                clause.QualifierArgument = second.Trim('*', '?', '+');

                qualifierText = second.After(clause.Clause);

                clause.RightQualifier = qualifierText switch
                {
                    "*" => ClauseQualifier.Optional,
                    "?" => ClauseQualifier.NoneToOne,
                    "+" => ClauseQualifier.OneOrMore,
                    _ => ClauseQualifier.None
                };
            }

            return clause;

        }

    }
}
