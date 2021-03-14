using Control.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Shift.Tests
{

    [TestClass]
    public class LexerServiceTest
    {

        GrammarService grammarService = new GrammarService();

        string literalsGrammar = $@"
{ShiftGrammar.literals}
{ShiftGrammar.STRING}
{ShiftGrammar.KEYWORD_TOKENS}
{ShiftGrammar.SYNTAX_TOKENS}
{ShiftGrammar.LASTCAPTURE_LITERALS}
{ShiftGrammar._fragments}
{ShiftGrammar.WHITESPACE}
";

        [TestMethod]
        public void IntegerLiteralTest()
        {

            var rules = grammarService.ConvertToBakedRules(literalsGrammar);

            var tokenStream = grammarService.ConvertToTokenStream("7", rules);

            tokenStream.First.Value.Rule.Name.Should().Be("INTEGER");

        }

        [TestMethod]
        public void BooleanTrueLiteralTest()
        {

            var rules = grammarService.ConvertToBakedRules(literalsGrammar);

            var tokenStream = grammarService.ConvertToTokenStream("true", rules);

            tokenStream.First.Value.Rule.Name.Should().Be("TRUE");

        }

        [TestMethod]
        public void BooleanFalseLiteralTest()
        {

            var rules = grammarService.ConvertToBakedRules(literalsGrammar);

            var tokenStream = grammarService.ConvertToTokenStream("false", rules);

            tokenStream.First.Value.Rule.Name.Should().Be("FALSE");

        }

        [TestMethod]
        public void StringTest()
        {

            var rules = grammarService.ConvertToBakedRules(literalsGrammar);

            var tokenStream = grammarService.ConvertToTokenStream("\"test\"", rules);

            tokenStream.First.Value.Rule.Name.Should().Be("STRING");

        }

        [TestMethod]
        public void CommentTest()
        {

            var rules = grammarService.ConvertToBakedRules(ShiftGrammar.COMMENT);

            var tokenStream = grammarService.ConvertToTokenStream("//This is a comment wiht an embedded string \"adsfadsf\" ", rules);

            tokenStream.Count.Should().Be(0);

        }

        [TestMethod]
        public void ReturnStringError()
        {

            //Realworld bug around strings being returned
            //Getting this as the token stream
            //        {RETURN} {UNLINKED:["]}{IDENTIFIER}{UNLINKED:["]}{SEMICOLON}
            //Discards have been reapplied to source formatting for the tab and spacing, unsure why it's lexing the identifier before the string

            var rules = grammarService.ConvertToBakedRules(literalsGrammar);

            var tokenStream = grammarService.ConvertToTokenStream("        return \"adsfadf\";", rules);

            tokenStream.First.Value.Rule.Name.Should().Be("RETURN");
            tokenStream.First.Next.Value.Rule.Name.Should().Be("STRING");
            tokenStream.First.Next.Next.Value.Rule.Name.Should().Be("SEMICOLON");


        }

    }
}
