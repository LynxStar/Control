using Control.Grammar;
using Control.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Control.Services
{

    public class RulesService
    {

        List<char> Whitespace = new List<char> { ' ', '\r', '\n', '\t' };
        List<char> Terminator = new List<char> { ')', ';', '|' };

        private readonly ReferenceLinkerService referenceLinkerService = new ReferenceLinkerService();

        public Dictionary<string, Rule> BuildRules(string grammarText)
        {

            var rules = new Dictionary<string, Rule>();

            var stream = new RulesStream { Source = grammarText };

            //Lex and parse rules
            while (stream.Index < stream.Source.Length)
            {

                Trim(stream);

                var rule = CaptureRule(stream);
                rules.Add(rule.Name, rule);

                Trim(stream);

            }

            rules = referenceLinkerService.LinkRules(rules);

            return rules;

        }

        public Rule CaptureRule(RulesStream stream)
        {

            var rule = new Rule();

            rule.RuleType = CaptureCommandType(stream);
            stream.Index++;
            rule.Name = CaptureCommandName(stream);
            stream.Index++;
            FindAndSkipPast(':', stream);
            rule.Options = CaptureOptions(stream);

            return rule;

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

        public string CaptureWord(RulesStream stream)
        {

            var start = stream.Index;
            var length = 0;

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];

                if (Whitespace.Contains(nextCharacter) || Terminator.Contains(nextCharacter))
                {
                    break;
                }

                stream.Index++;
                length++;

            }

            var capture = stream.Source.Substring(start, length);

            return capture;

        }

        public RuleType CaptureCommandType(RulesStream stream)
        {

            var typeString = CaptureWord(stream);

            return typeString switch
            {
                "form" => RuleType.Form,
                "token" => RuleType.Token,
                "fragment" => RuleType.Fragment,
                "discard" => RuleType.Discard,
                _ => throw new Exception($"Unknown rule {typeString}")
            };

        }

        public string CaptureCommandName(RulesStream stream)
        {

            var name = CaptureWord(stream);

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new Exception("This is empty, you're retarded");
            }

            return name;

        }

        public void FindAndSkipPast(char needle, RulesStream stream)
        {

            Trim(stream);

            var nextCharacter = stream.Source[stream.Index];

            if (nextCharacter != needle)
            {
                throw new Exception("Wtf is this?");
            }

            stream.Index++;

        }

        public List<RuleOption> CaptureOptions(RulesStream stream)
        {

            var definitions = new List<RuleOption>();

            while (true)
            {

                var definition = CaptureOption(stream, ';');
                definitions.Add(definition);

                var nextCharacter = stream.Source[stream.Index];
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

            return definitions;

        }

        public RuleOption CaptureOption(RulesStream stream, char terminator)
        {

            var definition = new RuleOption();

            Trim(stream);

            while (true)
            {

                var clause = CaptureClause(stream);

                definition.Clauses.Add(clause);

                Trim(stream);

                var nextCharacter = stream.Source[stream.Index];

                if (nextCharacter == '|' || nextCharacter == terminator)
                {
                    break;
                }

            }

            return definition;

        }

        public Clause CaptureClause(RulesStream stream)
        {

            var clause = new Clause();

            Trim(stream);

            var nextCharacter = stream.Source[stream.Index];
            stream.Index++;

            //Parens Stack
            if (nextCharacter == '(')
            {

                var option = CaptureOption(stream, ')'); 
                stream.Index++;

                clause.ClauseType = ClauseType.CaptureGroup;
                clause.CaptureGroup = new CaptureGroup
                {
                    Clauses = option.Clauses
                };

                nextCharacter = stream.Source[stream.Index];

                clause.CaptureGroup.Modifier = nextCharacter switch
                {
                    '*' => CaptureModifier.Optional,
                    '?' => CaptureModifier.NoneToOne,
                    '+' => CaptureModifier.OneOrMore,
                    _ => CaptureModifier.None
                };

                if (clause.CaptureGroup.Modifier != CaptureModifier.None)
                {
                    stream.Index++;
                }
            }

            //Literal
            else if (nextCharacter == '\'')
            {
                clause.Value = CaptureUntil(stream, '\'');
                clause.ClauseType = ClauseType.Literal;
            }

            //Regex
            else if (nextCharacter == '`')
            {
                clause.Value = CaptureUntil(stream, '`');
                clause.ClauseType = ClauseType.Regex;
            }
            else
            {
                stream.Index--;//The others assume the deliminating character was consumed, in this case we want to walk back
                clause.Value = CaptureWord(stream);
                clause.ClauseType = ClauseType.Reference;
            }

            if(!String.IsNullOrWhiteSpace(clause.Value))
            {
                clause.Value = clause.Value.Replace("\\\\", "\\");
            }

            return clause;

        }

        public string CaptureUntil(RulesStream stream, char terminator)
        {

            var start = stream.Index;
            var length = 0;

            while (stream.Index < stream.Source.Length)
            {

                var nextCharacter = stream.Source[stream.Index];


                //Handle escape characters
                if (nextCharacter == '\\')
                {

                    char escapedChar = stream.Source[stream.Index + 1];

                    if(escapedChar == '\\' || escapedChar == terminator)
                    {
                        stream.Index += 2;
                        length += 2;
                        continue;
                    }

                }

                if (nextCharacter == terminator)
                {
                    break;
                }

                stream.Index++;
                length++;

            }

            var capture = stream.Source.Substring(start, length);
            stream.Index++;

            return capture;

        }


    }

}
