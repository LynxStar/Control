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

namespace Control.Tests.Services
{

    [TestClass]
    public class RuleServiceTest
    {

        private readonly R2Service CUT = new R2Service();

        [TestMethod]
        public void SourceTest()
        {

            var source = 
@"rule source
	: structure
	| library
	| service
	;";

            var rules = CUT.BuildGrammarRules(source);

            var rule = rules.Single();

            rule.RuleType.Should().Be(RuleType.Rule);
            rule.Name.Should().Be("source");

            rule.Alternatives.Count().Should().Be(3);

            var alt1 = rule.Alternatives[0];

            alt1.RuleClauses.Count().Should().Be(1);
            alt1.RuleClauses[0].Clause.Should().Be("structure");
            alt1.RuleClauses[0].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);

            var alt2 = rule.Alternatives[1];

            alt2.RuleClauses.Count().Should().Be(1);
            alt2.RuleClauses[0].Clause.Should().Be("library");
            alt2.RuleClauses[0].IsLiteral.Should().BeFalse();
            alt2.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt2.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt2.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt2.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);

            var alt3 = rule.Alternatives[2];

            alt3.RuleClauses.Count().Should().Be(1);
            alt3.RuleClauses[0].Clause.Should().Be("service");
            alt3.RuleClauses[0].IsLiteral.Should().BeFalse();
            alt3.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt3.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt3.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt3.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);

        }
        
        [TestMethod]
        public void StructureTest()
        {

            var source = "rule structure : STRUCTURE identifier OPENSBRACKET field* CLOSESBRACKET;";

            var rules = CUT.BuildGrammarRules(source);

            var rule = rules.Single();

            rule.RuleType.Should().Be(RuleType.Rule);
            rule.Name.Should().Be("structure");

            rule.Alternatives.Count().Should().Be(1);

            var alt1 = rule.Alternatives[0];

            alt1.RuleClauses.Count().Should().Be(5);

            alt1.RuleClauses[0].Clause.Should().Be("STRUCTURE");
            alt1.RuleClauses[0].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);

            alt1.RuleClauses[1].Clause.Should().Be("identifier");
            alt1.RuleClauses[1].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[1].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[1].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[1].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[1].RightQualifier.Should().Be(ClauseQualifier.None);

            alt1.RuleClauses[2].Clause.Should().Be("OPENSBRACKET");
            alt1.RuleClauses[2].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[2].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[2].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[2].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[2].RightQualifier.Should().Be(ClauseQualifier.None);

            alt1.RuleClauses[3].Clause.Should().Be("field");
            alt1.RuleClauses[3].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[3].Qualifier.Should().Be(ClauseQualifier.Optional);
            alt1.RuleClauses[3].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[3].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[3].RightQualifier.Should().Be(ClauseQualifier.None);

            alt1.RuleClauses[4].Clause.Should().Be("CLOSESBRACKET");
            alt1.RuleClauses[4].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[4].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[4].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[4].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[4].RightQualifier.Should().Be(ClauseQualifier.None);

        }

        [TestMethod]
        public void ParametersTest()
        {

            var source = "rule parameters : OPENPARENS parameter*&COMMA CLOESPARENS;";

            var rules = CUT.BuildGrammarRules(source);

            var rule = rules.Single();

            rule.RuleType.Should().Be(RuleType.Rule);
            rule.Name.Should().Be("parameters");

            rule.Alternatives.Count().Should().Be(1);

            var alt1 = rule.Alternatives[0];

            alt1.RuleClauses.Count().Should().Be(3);

            alt1.RuleClauses[0].Clause.Should().Be("OPENPARENS");
            alt1.RuleClauses[0].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);
            
            alt1.RuleClauses[1].Clause.Should().Be("parameter");
            alt1.RuleClauses[1].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[1].Qualifier.Should().Be(ClauseQualifier.Optional);
            alt1.RuleClauses[1].DelimiterInUse.Should().BeTrue();
            alt1.RuleClauses[1].QualifierArgument.Should().Be("COMMA");
            alt1.RuleClauses[1].RightQualifier.Should().Be(ClauseQualifier.None);

            alt1.RuleClauses[2].Clause.Should().Be("CLOESPARENS");
            alt1.RuleClauses[2].IsLiteral.Should().BeFalse();
            alt1.RuleClauses[2].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[2].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[2].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[2].RightQualifier.Should().Be(ClauseQualifier.None);

        }

        [TestMethod]
        public void LibraryTest()
        {

            var source = "token LIBRARY : 'library';";

            var rules = CUT.BuildGrammarRules(source);

            var rule = rules.Single();

            rule.RuleType.Should().Be(RuleType.Token);
            rule.Name.Should().Be("LIBRARY");

            rule.Alternatives.Count().Should().Be(1);

            var alt1 = rule.Alternatives[0];

            alt1.RuleClauses.Count().Should().Be(1);

            alt1.RuleClauses[0].Clause.Should().Be("library");
            alt1.RuleClauses[0].IsLiteral.Should().BeTrue();
            alt1.RuleClauses[0].Qualifier.Should().Be(ClauseQualifier.None);
            alt1.RuleClauses[0].DelimiterInUse.Should().BeFalse();
            alt1.RuleClauses[0].QualifierArgument.Should().BeNull();
            alt1.RuleClauses[0].RightQualifier.Should().Be(ClauseQualifier.None);

        }

    }
}
