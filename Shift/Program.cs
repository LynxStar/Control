using Control.Services;
using Shift.Services;
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

            File.WriteAllText("./fullGrammar.txt", ShiftGrammar.FullGrammar);

            var source = await File.ReadAllTextAsync("./structureTest.st");

            var sourceTree = grammarService.ConvertTo<Concrete.Source>(grammar, source, EntryFormKey);

            var appService = new ApplicationService();

            var app = appService.MapSourceToApplication(sourceTree);

            var typeService = new TypeService();

            app = typeService.LinkExternalTypes(app);

            var compilerService = new CompilerService();

            compilerService.Compile(app);

        }
    
    }
}
