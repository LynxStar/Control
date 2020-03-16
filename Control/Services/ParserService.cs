using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Control.Services
{
    
    public class ParserService
    {

        public List<CodeFragment> Parse(string source, IEnumerable<GrammarRule> rules)
        {

            //Stage 1: DeFragment: Applies the fragments to the lexer tokens

            var tokenRules = DeFragment(source, rules);

            //Stage 2: Lexer
            //Stage 3: Parser

            return null;

        }

        public IEnumerable<TokenLexer> DeFragment(string source, IEnumerable<GrammarRule> rules)
        {

            var tokenRules = new List<TokenLexer>();

            var fragmentRules = rules
                .Where(x => x.RuleType == RuleType.Fragment)
                .ToDictionary(x => x.Name)
                ;

            var lexerRules = rules
                .Where(x => x.RuleType == RuleType.Token)
                ;

            foreach (var rule in lexerRules)
            {
                try
                {

                    var flattenedRule = FlattenRule(rule, fragmentRules);

                    Console.WriteLine($"Fragmentation: `{rule}` to `{flattenedRule}`");

                    //Work "((\")|([^"]))"
                    //Good "((\")|([^"]))*"

                    var tokenLexer = new TokenLexer
                    {
                        Name = rule.Name,
                        Regex = new Regex(flattenedRule)
                    };

                    tokenRules.Add(tokenLexer);

                }
                catch(Exception e)
                {

                }

            }

            return tokenRules;


        }

        public string FlattenRule(GrammarRule rule, Dictionary<string, GrammarRule> rules)
        {

            var flattenAlternatives = rule
                .Alternatives
                .Select(x => FlattenAlternative(x, rules))
                .ToList()
                ;

            //Only quote this up if there is alternatives
            if (flattenAlternatives.Count() > 1)
            {
                var flattened = flattenAlternatives
                   .Select(x => $"({x})")
                   .Aggregate((x, y) => $"{x}|{y}")
                   ;

                return $"({flattened})";

            }

            //Nothing to aggregate
            return flattenAlternatives.Single();

        }

        public string FlattenAlternative(Alternative alternative, Dictionary<string, GrammarRule> rules)
        {
            return alternative
                .RuleClauses
                .Select(x => FlattenRuleClause(x, rules))
                .Aggregate((x, y) => $"{x}{y}")
                ;
        }

        public string FlattenRuleClause(RuleClause ruleClause, Dictionary<string, GrammarRule> rules)
        {

            var clause = ruleClause.Clause;

            if (ruleClause.IsLiteral)
            {

                return Regex.Escape(clause);

            }
            else
            {
                return Flatten(ruleClause.Clause, rules);
            }

        }

        public string Flatten(string clause, Dictionary<string, GrammarRule> rules)
        {

            if(rules.ContainsKey(clause))
            {

                var fragment = rules[clause];

                var fragmentTest = FlattenRule(fragment, rules);

                return Flatten(fragmentTest, rules);

            }

            return clause;

        }

    }
}
