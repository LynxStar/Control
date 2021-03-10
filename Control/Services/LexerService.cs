using Control.Grammar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Control.Services
{

    public class LexerService
    {

        public LinkedList<Token> Tokenize(string source, Dictionary<string, Rule> rules)
        {

            var tokenStream = new LinkedList<Token>();

            tokenStream.AddLast(new Token { Capture = source });

            var tokenizableRules = rules
                .Where(x => x.Value.RuleType != RuleType.Form)
                .Select(x => x.Value)
                .ToList()
                ;

            foreach (var rule in tokenizableRules)
            {

                var tokenNode = tokenStream.First;

                while (tokenNode != null)
                {

                    var token = tokenNode.Value;

                    //If already tokenized skip forward
                    if (token.Rule is not null)
                    {
                        tokenNode = Next(tokenNode);
                        continue;
                    }

                    var regex = new Regex(rule.Regex);

                    var match = regex.Match(token.Capture);

                    //Raw text isn't this token
                    if (!match.Success)
                    {
                        tokenNode = Next(tokenNode);
                        continue;
                    }

                    //It's a match, eat this from the raw source
                    if (match.Index > 0)
                    {
                        var before = token.Capture.Substring(0, match.Index);

                        tokenStream.AddBefore(tokenNode, new Token { Capture = before });

                    }

                    var capture = token.Capture.Substring(match.Index, match.Length);

                    var captureToken = new Token
                    {
                        Rule = rule,
                        Capture = capture,
                    };

                    var captureNode = tokenStream.AddAfter(tokenNode, captureToken);
                    tokenStream.Remove(tokenNode);

                    var afterStart = match.Index + match.Length;
                    var endLength = token.Capture.Length - afterStart;

                    if (endLength > 0)
                    {

                        var after = token.Capture.Substring(afterStart, endLength);
                        tokenStream.AddAfter(captureNode, new Token { Capture = after });

                    }

                    tokenNode = captureNode.Next;

                }

            }

            DumpStream(tokenStream);

            var postDiscardStream = new LinkedList<Token>();

            foreach(var token in tokenStream)
            {
                if(token.Rule.RuleType != RuleType.Discard)
                {
                    postDiscardStream.AddLast(token);
                }
            }

            return postDiscardStream;

        }

        public void DumpStream(LinkedList<Token> stream)
        {

            var formattedBuilder = new StringBuilder();
            var rawBuilder = new StringBuilder();

            if(stream.Any(x => x.Rule is null))
            {
                throw new Exception("wtf bill?");
            }

            foreach (var node in stream)
            {

                if (node.Rule.RuleType == RuleType.Discard)
                {
                    formattedBuilder.Append(node.Capture);
                }
                else
                {
                    formattedBuilder.Append($"{{{node.Rule.Name}}}");
                    rawBuilder.AppendLine($"{{{node.Rule.Name}}}");
                }

            }

            if (File.Exists("./streamDump.txt"))
            {
                File.Delete("./streamDump.txt");
            }

            File.WriteAllText("./streamDump.txt", formattedBuilder.ToString());

            if (File.Exists("./rawDump.txt"))
            {
                File.Delete("./rawDump.txt");
            }

            File.WriteAllText("./rawDump.txt", rawBuilder.ToString());

        }

        public LinkedListNode<Token> Next(LinkedListNode<Token> node)
        {

            while (true)
            {

                node = node.Next;

                if (node == null)
                {
                    return node;
                }

                if (node.Value.Rule is null)
                {
                    return node;
                }

            }

        }

    }
}
