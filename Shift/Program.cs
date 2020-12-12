using Control.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shift
{
    
    public class Program
    {

        static async Task Main(string[] args)
        {

            var grammar = await File.ReadAllTextAsync("./Shift.ctrl");
            
            var parserService = new ParserService();
            var context = parserService.BuildParseContext(grammar);

            var exampleProgram = await File.ReadAllTextAsync("./Example.st");

            var sourceNode = context
                .SourceRules
                .Single(x => x.Name == "source")
                ;

            var parseTree = parserService.Parse(exampleProgram, sourceNode, context);

        }
    
    }
}
