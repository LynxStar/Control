using Control.Grammar;
using Control.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Control.Streams;

namespace Control.Tests.Services
{

    [TestClass]
    public class RuleServiceTest
    {

        private readonly RulesService CUT = new RulesService();

        [TestMethod]
        public void CaptureRuleTest()
        {

            var stream = new RulesStream { Source = "form parameter : type | identifier;" };

            var rule = CUT.CaptureRule(stream);

            rule.RuleType.Should().Be(RuleType.Form);
            rule.Name.Should().Be("parameter");

            var options = rule.Options;

            options.Count().Should().Be(2);

            var option1 = options[0];
            option1.Clauses.First().Value.Should().Be("type");
            option1.Clauses.First().ClauseType.Should().Be(ClauseType.Reference);

            var option2 = options[1];
            option2.Clauses.First().Value.Should().Be("identifier");
            option2.Clauses.First().ClauseType.Should().Be(ClauseType.Reference);

        }

        [TestMethod]
        public void DiscardTest()
        {

            var stream = new RulesStream { Source = "discard COMMENT : `\\/\\/.*?((?=\n)|(?=\r\n))`;" };

            var rule = CUT.CaptureRule(stream);

            rule.RuleType.Should().Be(RuleType.Discard);
            rule.Name.Should().Be("COMMENT");

            var options = rule.Options;

            options.Count().Should().Be(1);

            var option1 = options[0];
            option1.Clauses.First().Value.Should().Be("\\/\\/.*?((?=\n)|(?=\r\n))");
            option1.Clauses.First().ClauseType.Should().Be(ClauseType.Regex);

        }

        [TestMethod]
        public void ProcessGrammarTest()
        {

            var source = @"   
token STRUCTURE : 'structure';
token LIBRARY : 'library';
token SERVICE : 'service';
token EFFECT : 'effect';

token RETURN : 'return';

token OPENSBRACKET : '{';
token CLOSESBRACKET : '}';
token OPENPARENS : '(';
token CLOESPARENS : ')';


";

            var grammar = CUT.BuildRules(source);

            grammar.Values.Count().Should().Be(9);

        }

        [TestMethod]
        public void ReferenceLinkTest()
        {

            var source = @"   
form foo : BAZ BAZ;
form bar : foo | BAZ;
token BAZ : 'baz';


";

            var grammar = CUT.BuildRules(source);

        }

    }
}
