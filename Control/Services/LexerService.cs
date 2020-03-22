using Control.Grammar;
using System;
using System.Collections.Generic;
using System.Text;
using Control;
using System.IO;

namespace Control.Services
{
    

    public class LexerService
    {

        public void Tokenize(string source, IEnumerable<TokenRegex> tokenRules)
        {

            var tokenStream = new LinkedList<Token>();

            tokenStream.AddLast(new Token { Name = "@@@RAWSOURCE@@@", Capture = source, IsRaw = true });

            foreach (var tokenRule in tokenRules)
            {

                var tokenNode = tokenStream.First;

                while (tokenNode != null)
                {

                    Visualize(tokenStream, tokenNode, tokenRule);
                    DumpStream(tokenStream);

                    var token = tokenNode.Value;

                    if (!token.IsRaw)
                    {
                        tokenNode = Next(tokenNode);
                        continue;
                    }

                    var match = tokenRule.Regex.Match(token.Capture);

                    if(!match.Success)
                    {
                        tokenNode = Next(tokenNode);
                        continue;
                    }

                    if (match.Index > 0)
                    {
                        var before = token.Capture.Substring(0, match.Index);

                        tokenStream.AddBefore(tokenNode, new Token { Name = "@@@RAWSOURCE@@@", Capture = before, IsRaw = true });

                    }

                    var capture = token.Capture.Substring(match.Index, match.Length);

                    var captureToken = new Token
                    {
                        Name = tokenRule.Name,
                        Capture = capture,
                        IsRaw = false
                    };

                    var captureNode = tokenStream.AddAfter(tokenNode, captureToken);
                    tokenStream.Remove(tokenNode);

                    var afterStart = match.Index + match.Length;
                    var endLength = token.Capture.Length - afterStart;

                    if(endLength > 0)
                    {

                        var after = token.Capture.Substring(afterStart, endLength);
                        tokenStream.AddAfter(captureNode, new Token { Name = "@@@RAWSOURCE@@@", Capture = after, IsRaw = true });

                    }

                    tokenNode = captureNode.Next;

                }

            }

        }

        public void Visualize(LinkedList<Token> stream, LinkedListNode<Token> current, TokenRegex tokenRegex)
        {

            Console.Clear();

            var first = stream.First;

            Console.WriteLine($"Looking for: {tokenRegex.Name} with `{tokenRegex.Regex}`");
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine();

            while (first != null)
            {

                var hereMarker = first == current
                    ? "===> "
                    : "     "
                    ;

                Console.WriteLine($"{hereMarker}{first.Value.Name.PadRight(30)}||{first.Value.Capture.Preview(30)}");

                first = first.Next;

            }

        }

        public void DumpStream(LinkedList<Token> stream)
        {

            var builder = new StringBuilder();

            foreach(var node in stream)
            {

                if(node.Name == "WHITESPACE")
                {
                    builder.Append(node.Capture);
                }

                builder.Append($"{{{node.Name}}}");

            }

            if(File.Exists("./streamDump.txt"))
            {
                File.Delete("./streamDump.txt");
            }

            File.WriteAllText("./streamDump.txt", builder.ToString());

        }

        public LinkedListNode<Token> Next(LinkedListNode<Token> node)
        {

            while(true)
            {

                node = node.Next;

                if(node == null)
                {
                    return node;
                }

                if (node.Value.IsRaw)
                {
                    return node;
                }

            }

        }

    }
}
