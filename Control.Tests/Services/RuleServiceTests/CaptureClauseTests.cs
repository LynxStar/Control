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

namespace Control.Tests.Services.RuleServiceTests
{

    [TestClass]
    public class CaptureClauseTests
    {

        private readonly RulesService CUT = new RulesService();

        [TestMethod]
        public void ClauseBasicTest()
        {

            var stream = new RulesStream { Source = "'basic';" };

            var clause = CUT.CaptureClause(stream);

            clause.Value.Should().Be("basic");
            clause.ClauseType.Should().Be(ClauseType.Literal);
            stream.Index.Should().Be(7);

        }

        [TestMethod]
        public void ClauseRegexTest()
        {

            var stream = new RulesStream { Source = "`basic`;" };

            var clause = CUT.CaptureClause(stream);

            clause.Value.Should().Be("basic");
            clause.ClauseType.Should().Be(ClauseType.Regex);
            stream.Index.Should().Be(7);

        }

        [TestMethod]
        public void ClauseReferenceTest()
        {

            var stream = new RulesStream { Source = " basic ;" };

            var clause = CUT.CaptureClause(stream);

            clause.Value.Should().Be("basic");
            clause.ClauseType.Should().Be(ClauseType.Reference);
            stream.Index.Should().Be(6);

        }

        [TestMethod]
        public void ClauseWhitespaceTest()
        {

            var stream = new RulesStream { Source = "  'basic  '  ;   " };

            var clause = CUT.CaptureClause(stream);

            clause.Value.Should().Be("basic  ");
            clause.ClauseType.Should().Be(ClauseType.Literal);
            stream.Index.Should().Be(11);

        }

        [TestMethod]
        public void ClauseParensTest()
        {

            var stream = new RulesStream { Source = "( `HONK` )  ; form HELLO_THERE" };

            var clause = CUT.CaptureClause(stream);

            clause.ClauseType.Should().Be(ClauseType.CaptureGroup);
            clause.CaptureGroup.Modifier.Should().Be(CaptureModifier.None);
            clause.CaptureGroup.Clauses.Count().Should().Be(1);

            var parensClause = clause.CaptureGroup.Clauses.First();

            parensClause.Value.Should().Be("HONK");
            parensClause.ClauseType.Should().Be(ClauseType.Regex);

            stream.Index.Should().Be(10);

        }

        [TestMethod]
        public void ClauseParensOptionalModifierTest()
        {

            var stream = new RulesStream { Source = "( `HONK` )*;" };

            var clause = CUT.CaptureClause(stream);
            clause.CaptureGroup.Modifier.Should().Be(CaptureModifier.Optional);

        }

        [TestMethod]
        public void ClauseParensNoneToOneModifierTest()
        {

            var stream = new RulesStream { Source = "( `HONK` )?;" };

            var clause = CUT.CaptureClause(stream);
            clause.CaptureGroup.Modifier.Should().Be(CaptureModifier.NoneToOne);

        }

        [TestMethod]
        public void ClauseParensOneOrMoreModifierTest()
        {

            var stream = new RulesStream { Source = "(`HONK`)+;" };

            var clause = CUT.CaptureClause(stream);
            clause.CaptureGroup.Modifier.Should().Be(CaptureModifier.OneOrMore);

        }

        [TestMethod]
        public void ClauseComplexNestedParensTest()
        {

            var stream = new RulesStream { Source = "  ( `HONK` (BIG HONK BEDS)?)+;" };

            var clause = CUT.CaptureClause(stream);
            clause.ClauseType.Should().Be(ClauseType.CaptureGroup);
            clause.CaptureGroup.Modifier.Should().Be(CaptureModifier.OneOrMore);
            clause.CaptureGroup.Clauses.Count().Should().Be(2);

            var outerParensFirst = clause.CaptureGroup.Clauses.First();
            outerParensFirst.Value.Should().Be("HONK");
            outerParensFirst.ClauseType.Should().Be(ClauseType.Regex);

            var outerParensSecond = clause.CaptureGroup.Clauses.Skip(1).Take(1).First();
            outerParensSecond.ClauseType.Should().Be(ClauseType.CaptureGroup);
            outerParensSecond.CaptureGroup.Modifier.Should().Be(CaptureModifier.NoneToOne);
            outerParensSecond.CaptureGroup.Clauses.Count().Should().Be(3);

            var innerClauseOne = outerParensSecond.CaptureGroup.Clauses[0];
            innerClauseOne.Value.Should().Be("BIG");
            innerClauseOne.ClauseType.Should().Be(ClauseType.Reference);

            var innerClauseTwo = outerParensSecond.CaptureGroup.Clauses[1];
            innerClauseTwo.Value.Should().Be("HONK");
            innerClauseTwo.ClauseType.Should().Be(ClauseType.Reference);

            var innerClauseThree = outerParensSecond.CaptureGroup.Clauses[2];
            innerClauseThree.Value.Should().Be("BEDS");
            innerClauseThree.ClauseType.Should().Be(ClauseType.Reference);

            stream.Index.Should().Be(29);
        }

    }
}
