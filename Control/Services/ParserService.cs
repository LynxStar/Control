using Control.Grammar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Services
{
    
    public class ParseContext
    {
        public LinkedListNode<Token> Token { get; set; }
    }

    public class ParserService
    {

        private int CaptureGroupRuleCount = 0;

        public SyntaxNode ParseTokenStream(LinkedList<Token> tokens, Rule entryRule)
        {

            var context = new ParseContext
            {
                Token = tokens.First
            };

            var node = ParseRule(entryRule, context);

            DumpAST(node);

            return node;

        }

        public void DumpAST(SyntaxNode node)
        {

            var builder = new StringBuilder();

            DumpNode(node, builder, "");

            if (File.Exists("./ASTDump.txt"))
            {
                File.Delete("./ASTDump.txt");
            }

            File.WriteAllText("./ASTDump.txt", builder.ToString());

        }

        public void DumpNode(SyntaxNode node, StringBuilder builder, string tabDepth)
        {

            var type = node.Rule.RuleType == RuleType.Form
                ? "Form"
                : "Token"
                ;

            builder.AppendLine($"{tabDepth}{type} - {node.Rule.Name}: {node.Capture}");

            foreach(var inner in node.SyntaxNodes)
            {
                DumpNode(inner, builder, $"{tabDepth}\t");
            }

        }

        public SyntaxNode ParseRule(Rule rule, ParseContext context)
        {

            var node = new SyntaxNode
            {
                Rule = rule,
            };

            foreach(var option in rule.Options)
            {

                var currentToken = context.Token;
                var optionNodes = TryOption(option, context);

                if(optionNodes is not null)
                {
                    node.SyntaxNodes = optionNodes;
                    break;
                }

                context.Token = currentToken;

            }

            if(!node.SyntaxNodes.Any())
            {
                return null;
            }

            return node;
        }

        public List<SyntaxNode> TryOption(RuleOption option, ParseContext context)
        {

            var nodes = new List<SyntaxNode>();

            foreach(var clause in option.Clauses)
            {

                var node = MatchClause(clause, context);

                if(node is null)
                {
                    return null;
                }

                nodes.Add(node);

            }

            return nodes;

        }

        public SyntaxNode MatchClause(Clause clause, ParseContext context)
        {

            if (clause.ClauseType == ClauseType.Reference)
            {
                return MatchReference(clause.Reference, context);
            }
            else if (clause.ClauseType == ClauseType.CaptureGroup)
            {
                return MatchCaptureGroup(clause, context);

            }
            else
            {
                throw new Exception("wtf happened that caused this?");
            }

        }

        private SyntaxNode MatchCaptureGroup(Clause clause, ParseContext context)
        {

            var currentToken = context.Token;

            var required = clause.CaptureGroup.Modifier is CaptureModifier.None or CaptureModifier.OneOrMore;
            var allowMultiple = clause.CaptureGroup.Modifier is CaptureModifier.OneOrMore or CaptureModifier.Optional;

            List<List<SyntaxNode>> matches = new List<List<SyntaxNode>>();

            while (true)
            {

                var optionNodes = TryOption(clause.CaptureGroup, context);

                if (optionNodes is null)
                {
                    break;
                }

                matches.Add(optionNodes);

                if (!allowMultiple)
                {
                    break;
                }

            }

            if (required && !matches.Any())
            {
                context.Token = currentToken;
                return null;
            }

            var cgr = new Rule
            {
                RuleType = RuleType.Form,
                Name = $"CGR ({clause.CaptureGroup.Modifier}) - {CaptureGroupRuleCount++}",
                Options = new List<RuleOption> { clause.CaptureGroup }
            };

            var node = new SyntaxNode
            {
                Rule = cgr,
            };

            if (allowMultiple)
            {

                var innerMatch = 1;

                foreach (var match in matches)
                {
                    var innerCgr = new Rule
                    {
                        RuleType = RuleType.Form,
                        Name = $"CGR ({clause.CaptureGroup.Modifier}) - {CaptureGroupRuleCount} : Capture {innerMatch++}",
                        Options = new List<RuleOption> { clause.CaptureGroup }
                    };

                    var innerNode = new SyntaxNode
                    { 
                        Rule = innerCgr,
                        SyntaxNodes = match
                    };

                    node.SyntaxNodes.Add(innerNode);

                }
            }
            else
            {
                node.SyntaxNodes = matches.FirstOrDefault() ?? Enumerable.Empty<SyntaxNode>().ToList();
            }

            return node;
        }

        public SyntaxNode MatchReference(Rule reference, ParseContext context)
        {

            if(reference.RuleType == RuleType.Form)
            {
                return ParseRule(reference, context);
            }
            else if(reference.RuleType == RuleType.Token)
            {

                var token = context.Token.Value;

                if (token.Rule.Name == reference.Name)
                {
                    var node = new SyntaxNode
                    {
                        Rule = reference,
                        Capture = token.Capture
                    };

                    context.Token = context.Token.Next;

                    return node;

                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exception("How did you reference this?");
            }

        }

    }

}
