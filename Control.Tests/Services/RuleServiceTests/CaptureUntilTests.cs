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
    public class CaptureUntilTests
    {

        private readonly RulesService CUT = new RulesService();

        [TestMethod]
        public void CaptureUntilBasicTest()
        {

            var stream = new RulesStream { Source = "basic'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic");

        }

        [TestMethod]
        public void CaptureUntilWhitespaceTest()
        {

            var stream = new RulesStream { Source = "basic asdfasdfa asdfasdf'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic asdfasdfa asdfasdf");

        }

        [TestMethod]
        public void CaptureUntilMixedTerminatorTest()
        {

            var stream = new RulesStream { Source = "basic asd````fa'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic asd````fa");

        }

        [TestMethod]
        public void CaptureUntilWithEscapedCharacterTest()
        {

            var stream = new RulesStream { Source = "basic\\'Escape'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic\\'Escape");

        }

        [TestMethod]
        public void CaptureUntilDoubleEscapedDoesNotEscapeTest()
        {

            var stream = new RulesStream { Source = "basic\\\\'Escape'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic\\\\");

        }

        [TestMethod]
        public void CaptureUntilDoubleEscapedThenEscapeTest()
        {

            var stream = new RulesStream { Source = "basic\\\\\\'Escape'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic\\\\\\'Escape");

        }


        [TestMethod]
        public void CaptureUntilEscapeOther()
        {

            var stream = new RulesStream { Source = "basic\t'Escape'" };

            var capture = CUT.CaptureUntil(stream, '\'');

            capture.Should().Be("basic\t");

        }


        [TestMethod]
        public void CaptureUntilOtherTerminator()
        {

            var stream = new RulesStream { Source = "basic`" };

            var capture = CUT.CaptureUntil(stream, '`');

            capture.Should().Be("basic");

        }

        [TestMethod]
        public void CaptureUntilActuallyCapturesUntil()
        {

            var stream = new RulesStream { Source = "basic`all of this other stuff shouldn't be here" };

            var capture = CUT.CaptureUntil(stream, '`');

            capture.Should().Be("basic");
            stream.Index.Should().Be(6);

        }

        [TestMethod]
        public void CaptureUntilCorrectlyMoveIndex()
        {

            var stream = new RulesStream { Source = "ba\\`sic\\\\`readyIndex" };

            var capture = CUT.CaptureUntil(stream, '`');

            capture.Should().Be("ba\\`sic\\\\");
            stream.Index.Should().Be(10);

        }

    }
}
