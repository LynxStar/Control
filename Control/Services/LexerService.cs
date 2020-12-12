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

        public LinkedList<Token> Tokenize(string source, IEnumerable<TokenRegex> tokenRules)
        {

            var tokenStream = new LinkedList<Token>();

            tokenStream.AddLast(new Token { Name = "@@@RAWSOURCE@@@", Capture = source, IsRaw = true, IsNoop = false });
                       
            var lastDisplay = DateTime.UtcNow;

            foreach (var tokenRule in tokenRules)
            {

                var tokenNode = tokenStream.First;

                while (tokenNode != null)
                {

                    if (DateTime.UtcNow - lastDisplay > TimeSpan.FromMilliseconds(250))
                    {
                        lastDisplay = DateTime.UtcNow;
                        Visualize(tokenStream, tokenNode, tokenRule);

                    }

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

                        tokenStream.AddBefore(tokenNode, new Token { Name = "@@@RAWSOURCE@@@", Capture = before, IsRaw = true, IsNoop = false });

                    }

                    var capture = token.Capture.Substring(match.Index, match.Length);

                    var captureToken = new Token
                    {
                        Name = tokenRule.Name,
                        Capture = capture,
                        IsRaw = false,
                        IsNoop = tokenRule.IsNoop
                    };

                    var captureNode = tokenStream.AddAfter(tokenNode, captureToken);
                    tokenStream.Remove(tokenNode);

                    var afterStart = match.Index + match.Length;
                    var endLength = token.Capture.Length - afterStart;

                    if(endLength > 0)
                    {

                        var after = token.Capture.Substring(afterStart, endLength);
                        tokenStream.AddAfter(captureNode, new Token { Name = "@@@RAWSOURCE@@@", Capture = after, IsRaw = true, IsNoop = false });

                    }

                    tokenNode = captureNode.Next;

                }

            }

            DumpStream(tokenStream);

            return tokenStream;

        }

        public void Visualize(LinkedList<Token> stream, LinkedListNode<Token> current, TokenRegex tokenRegex)
        {

            Console.Clear();

            Console.WriteLine($"Looking for: {tokenRegex.Name} with `{tokenRegex.Regex}`");
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine();

            var printNow = current.PreviousBy(5);

            for(var i = 0; i < 10; i++)
            {
                var hereMarker = printNow == current
                    ? "===> "
                    : "     "
                    ;

                Console.WriteLine($"{hereMarker}{printNow.Value.Name.PadRight(30)}||{printNow.Value.Capture.Preview(30)}");

                printNow = printNow.Next;

                if(printNow == null)
                {
                    break;
                }

            }

        }

        public void DumpStream(LinkedList<Token> stream)
        {

            var formattedBuilder = new StringBuilder();
            var rawBuilder = new StringBuilder();

            foreach (var node in stream)
            {

                if(node.Name == "WHITESPACE")
                {
                    formattedBuilder.Append(node.Capture);
                }
                else
                {
                    formattedBuilder.Append($"{{{node.Name}}}");
                    rawBuilder.AppendLine($"{{{node.Name}}}");
                }

            }

            if(File.Exists("./streamDump.txt"))
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
