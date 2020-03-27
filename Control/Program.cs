using Control.Grammar;
using Control.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Control
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Program().Run();
        }

        public async Task Run()
        {

            var exampleProgram = await File.ReadAllTextAsync("./Example.st");

            var grammar = await File.ReadAllTextAsync("./Shift.ctrl");

            var grammarRulesService = new RulesService();

            var rules = grammarRulesService.BuildGrammarRules(grammar);

            var parserService = new ParserService();

            var code = parserService.Parse(exampleProgram, rules);


        }

        

    }
}
