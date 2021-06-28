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

namespace Control.Tests.Services.RuleServiceTests
{

    [TestClass]
    public class CaptureOptionsTests
    {

        private readonly RulesService CUT = new RulesService();

        [TestMethod]
        public void CaptureOptionTest()
        {

            var stream = new RulesStream { Source = "BIG HONK BEDS   ;" };

            var option = CUT.CaptureOption(stream, ';');

            var innerClauseOne = option.Clauses[0];
            innerClauseOne.Value.Should().Be("BIG");
            innerClauseOne.ClauseType.Should().Be(ClauseType.Reference);

            var innerClauseTwo = option.Clauses[1];
            innerClauseTwo.Value.Should().Be("HONK");
            innerClauseTwo.ClauseType.Should().Be(ClauseType.Reference);

            var innerClauseThree = option.Clauses[2];
            innerClauseThree.Value.Should().Be("BEDS");
            innerClauseThree.ClauseType.Should().Be(ClauseType.Reference);

            stream.Index.Should().Be(16);

        }

        [TestMethod]
        public void CaptureOptionsTest()
        {

            var stream = new RulesStream { Source = "BIG | HONK | BEDS BIGBED  ;   token penguin" };

            var options = CUT.CaptureOptions(stream);

            options.Count().Should().Be(3);

            var option1 = options[0];
            option1.Clauses.First().Value.Should().Be("BIG");
            option1.Clauses.First().ClauseType.Should().Be(ClauseType.Reference);


            var option2 = options[1];
            option2.Clauses.First().Value.Should().Be("HONK");
            option2.Clauses.First().ClauseType.Should().Be(ClauseType.Reference);

            var option3 = options[2];

            option3.Clauses.Count().Should().Be(2);

            var o3c1 = option3.Clauses[0];
            o3c1.Value.Should().Be("BEDS");
            o3c1.ClauseType.Should().Be(ClauseType.Reference);

            var o3c2 = option3.Clauses[1];
            o3c2.Value.Should().Be("BIGBED");
            o3c2.ClauseType.Should().Be(ClauseType.Reference);

            stream.Index.Should().Be(27);

        }

        [TestMethod]
        public void CaptureEscapeCharacterIsTheRegexClauseItselfTest()
        {

            var stream = new RulesStream { Source = "`\\\\` DOUBLEQUOTE;" };

            var options = CUT.CaptureOptions(stream);

            options.Count().Should().Be(1);
            options.First().Clauses.Count().Should().Be(2);

            var clause1 = options[0].Clauses[0];
            clause1.Value.Should().Be("\\");
            clause1.ClauseType.Should().Be(ClauseType.Regex);

            var clause2 = options[0].Clauses[1];
            clause2.Value.Should().Be("DOUBLEQUOTE");
            clause2.ClauseType.Should().Be(ClauseType.Reference);

        }

    }
}
