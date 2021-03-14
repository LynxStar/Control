using Control.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shift
{
    
    public class Program
    {

        private static readonly String EntryFormKey = "source";

        static async Task Main(string[] args)
        {

            var grammarService = new GrammarService();

            var grammar = ShiftGrammar.FullGrammar;
            var source = await File.ReadAllTextAsync("./structureTest.st");

            var rootNode = grammarService.ConvertToAST(grammar, source, EntryFormKey);

            var mapper = new SyntaxMapper();

            var app = mapper.MapApplication(rootNode);

        }
    
    }
}
