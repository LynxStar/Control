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

        private readonly RulesService rulesService = new RulesService();
        private readonly FragmentService _fragmentService = new FragmentService();
        private readonly LexerService _lexer = new LexerService();

        public ParseContext BuildParseContext(string grammar)
        {

            var rules = rulesService.BuildGrammarRules(grammar);

            var tokenRules = _fragmentService.BuildTokenRegex(rules);

            var sourceRules = rules
                .Where(x => x.RuleType == RuleType.Rule || x.RuleType == RuleType.Token)
                .ToDictionary(x => x.Name)
                ;

            return new ParseContext 
            {
                SourceRules = sourceRules,
                TokenRules = tokenRules
            };

        }

        public List<RuleNode> Parse(string source, GrammarRule sourceRule, ParseContext context)
        {

            var tokenStream = _lexer.Tokenize(source, context.TokenRules);

            var nodes = new List<RuleNode>();

            context.CurrentNode = tokenStream.First;

            while(context.CurrentNode != null)
            {

                var node = ParseRuleNode(sourceRule, context);
                nodes.Add(node);

            }

            return nodes;

        }

        public RuleNode ParseRuleNode(GrammarRule rule, ParseContext context)
        {

            foreach(var alternative in rule.Alternatives)
            {

                var alternativeNode = ParseAlternativeNode(alternative, context);

                if(alternativeNode != null)
                {

                    var node = new RuleNode
                    {
                        Rule = rule
                    };

                }


            }

            return null;

        }

        public RuleNode ParseAlternativeNode(Alternative alternative, ParseContext context)
        {

            foreach(var ruleClause in alternative.RuleClauses)
            {

                var clauseNode = ParseClauseNode(ruleClause, context);

            }

            return null;

        }

        public RuleNode ParseClauseNode(RuleClause ruleClause, ParseContext context)
        {

            var clauseName = ruleClause.Clause;

            var sourceRule = context.SourceRules[clauseName];

            if (sourceRule.RuleType == RuleType.Token)
            {

                if (context.CurrentNode.Value.Name == clauseName)
                {

                    var capturedNode = new RuleNode
                    {
                        Rule = sourceRule,
                        Token = context.CurrentNode.Value
                    };

                    context.CurrentNode = context.CurrentNode.Next;

                    return capturedNode;

                }

                return null;

            }
            else if (sourceRule.RuleType == RuleType.Rule)
            {

            }

            return null;

        }

    }
}
