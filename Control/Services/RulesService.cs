using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Text;

namespace Control.Services
{

    public class RulesStream
    {

        public string Source { get; set; }
        public int Index { get; set; }


    }
    
    public class RulesService
    {

        List<char> Whitespace = new List<char> { ' ', '\r', '\n', '\t' };
        List<char> ClauseQualifiers = new List<char> { '*', '+', '?' };

        public List<GrammarRule> BuildGrammarRules(string grammarText)
        {

            var grammarRules = new List<GrammarRule>();

            var rulesStream = new RulesStream { Source = grammarText };

            while(rulesStream.Index < rulesStream.Source.Length)
            {

                var grammarRule = new GrammarRule();
                
                grammarRule.RuleType = CaptureCommandType(rulesStream);
                grammarRule.Name = CaptureCommandName(rulesStream);
                grammarRule.Alternatives = CaptureAlternatives(rulesStream);

                grammarRules.Add(grammarRule);
            }

            return grammarRules;

        }

        public void Trim(RulesStream stream)
        {

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (Whitespace.Contains(nextCharacter))
                {
                    stream.Index++;
                    continue;
                }

                break;

            }

        }

        public RuleType CaptureCommandType(RulesStream stream)
        {

            Trim(stream);

            var start = stream.Index;
            var length = 0;

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (Whitespace.Contains(nextCharacter))
                {
                    break;
                }

                stream.Index++;
                length++;

            }

            var typeString = stream.Source.Substring(start, length);

            return typeString switch 
            { 
                "rule" => RuleType.Rule,
                "token" => RuleType.Token,
                "fragment" => RuleType.Fragment,
                "noop" => RuleType.Noop,
                _ => throw new Exception($"Unknown rule {typeString}")
            };

        }

        public string CaptureCommandName(RulesStream stream)
        {

            Trim(stream);

            var start = stream.Index;
            var length = 0;

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (Whitespace.Contains(nextCharacter))
                {
                    break;
                }

                stream.Index++;
                length++;

            }

            var name = stream.Source.Substring(start, length);

            return name;

        }

        public List<Alternative> CaptureAlternatives(RulesStream stream)
        {

            var alternatives = new List<Alternative>();

            Trim(stream);

            var nextCharacter = stream.Source[stream.Index];

            if(nextCharacter != ':')
            {
                throw new Exception("Wtf is this?");
            }

            stream.Index++;

            while (stream.Index < stream.Source.Length)
            {

                var alternative = CaptureAlternative(stream);
                alternatives.Add(alternative);

                nextCharacter = stream.Source[stream.Index];
                stream.Index++;

                if (nextCharacter == '|')
                {
                    continue;
                }

                if (nextCharacter == ';')
                {
                    break;
                }

            }

            return alternatives;

        }

        public Alternative CaptureAlternative(RulesStream stream)
        {

            var alternative = new Alternative();

            Trim(stream);

            while(stream.Index < stream.Source.Length)
            {

                var clause = CaptureRuleClause(stream);

                alternative.RuleClauses.Add(clause);

                Trim(stream);

                var nextCharacter = stream.Source[stream.Index];

                if (nextCharacter == '|' || nextCharacter == ';')
                {
                    break;
                }

            }

            return alternative;

        }

        public RuleClause CaptureRuleClause(RulesStream stream)
        {

            var clause = new RuleClause();

            Trim(stream);

            var left = CaptureClauseSide(stream);

            clause.Clause = left.Clause;
            clause.Qualifier = left.Qualifier;
            clause.IsLiteral = left.IsLiteral;
            clause.IsRegex = left.IsRegex;

            var nextCharacter = stream.Source[stream.Index];

            if (nextCharacter == '&')
            {
                stream.Index++;
                var right = CaptureClauseSide(stream);

                clause.DelimiterInUse = true;
                clause.QualifierArgument = right.Clause;
                clause.RightQualifier = right.RightQualifier;
            }

            return clause;

        }

        public RuleClause CaptureClauseSide(RulesStream stream)
        {
            var clause = new RuleClause();

            var start = stream.Index;
            var length = 0;


            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (nextCharacter == '|' || nextCharacter == ';')
                {
                    break;
                }

                if (nextCharacter == '\'')
                {

                    stream.Index++;
                    clause.IsLiteral = true;
                    clause.Clause = CaptureStringLiteral(stream, '\'');
                    return clause;

                }

                if (nextCharacter == '`')
                {
                    stream.Index++;
                    clause.IsRegex = true;
                    clause.Clause = CaptureStringLiteral(stream, '`');
                    return clause;
                }

                if (Whitespace.Contains(nextCharacter))
                {
                    clause.Qualifier = ClauseQualifier.None;
                    break;
                }

                if (ClauseQualifiers.Contains(nextCharacter))
                {

                    clause.Qualifier = nextCharacter switch
                    {
                        '*' => ClauseQualifier.Optional,
                        '?' => ClauseQualifier.NoneToOne,
                        '+' => ClauseQualifier.OneOrMore,
                        _ => throw new Exception("What is this symbol?")
                    };

                    stream.Index++;

                    break;
                }

                stream.Index++;
                length++;

            }

            var clauseText = stream.Source.Substring(start, length);

            clause.Clause = clauseText;

            return clause;
        }

        public string CaptureStringLiteral(RulesStream stream, char delimiter)
        {

            var start = stream.Index;
            var length = 0;

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (nextCharacter == delimiter)
                {
                    stream.Index++;
                    break;
                }

                stream.Index++;
                length++;

            }

            var literalText = stream.Source.Substring(start, length);

            return literalText;

        }

    }

}
