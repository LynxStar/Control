using Control.Services;
using Shift.Intermediate;
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

            var typeContext = new TypeContext();

            var concreteMapper = new ConcreteMapper();

            concreteMapper.MapSourceToApplication(sourceTree, typeContext);

            var enforcer = new TypeEnforcer();

            enforcer.TypeCheckApplication(typeContext);

            var compilerService = new CompilerService();

            compilerService.Compile(typeContext.Application);

        }
    
    }
}
