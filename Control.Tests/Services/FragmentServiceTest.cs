using Control.Grammar;
using Control.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Tests.Services
{
    
    [TestClass]
    public class FragmentServiceTest
    {

        private readonly FragmentService CUT = new FragmentService();

        [TestMethod]
        public void LiteralRegexTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.Literal,
                Value = "structure"
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("structure");

        }

        [TestMethod]
        public void LiteralNeedingEscapeTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.Literal,
                Value = "honk(f)"
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("honk\\(f\\)");

        }

        [TestMethod]
        public void RegexDoesNotEscapeTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.Regex,
                Value = "honk(f)"
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("honk(f)");

        }

        [TestMethod]
        public void OptionConcatenationTest()
        {

            var option = new RuleOption
            {
                Clauses = new List<Clause> 
                {
                    new Clause{ ClauseType = ClauseType.Literal, Value = "structure" },
                    new Clause{ ClauseType = ClauseType.Regex, Value = "honk(f)" },
                    new Clause{ ClauseType = ClauseType.Literal, Value = "honk(f)" }
                }
            };

            var regex = CUT.BuildOptionRegex(option, null);

            regex.Should().Be("structurehonk(f)honk\\(f\\)");

        }

        [TestMethod]
        public void SingleOptionRuleTest()
        {

            var rule = new Rule
            {
                RuleType = RuleType.Token,
                Options = new List<RuleOption>
                {
                    new RuleOption
                    {
                        Clauses = new List<Clause>
                        {
                            new Clause { ClauseType = ClauseType.Literal, Value = "structure" },
                            new Clause { ClauseType = ClauseType.Regex, Value = "honk(f)" },
                            new Clause { ClauseType = ClauseType.Literal, Value = "honk(f)" }
                        }

                    }
                }
            };

            var regex = CUT.BuildRuleRegex(rule, null);

            regex.Should().Be("structurehonk(f)honk\\(f\\)");

        }

        [TestMethod]
        public void BasicMultiOptionRuleTest()
        {

            var rule = new Rule
            {
                Name = "BASICTEST",
                RuleType = RuleType.Token,
                Options = new List<RuleOption>
                {
                    new RuleOption
                    {
                        Clauses = new List<Clause>
                        {
                            new Clause { ClauseType = ClauseType.Literal, Value = "left" },
                        }

                    },
                    new RuleOption
                    {
                        Clauses = new List<Clause>
                        {
                            new Clause { ClauseType = ClauseType.Literal, Value = "right" },
                        }

                    }
                }
            };

            var regex = CUT.BuildRuleRegex(rule, null);

            regex.Should().Be("(left)|(right)");

        }

        [TestMethod]
        public void ReferenceClauseTest()
        {

            var rules = new List<Rule>()
            {
                new Rule
                {
                    Name = "BASICTEST",
                    RuleType = RuleType.Token,
                    Options = new List<RuleOption>
                    {
                        new RuleOption
                        {
                            Clauses = new List<Clause>
                            {
                                new Clause { ClauseType = ClauseType.Literal, Value = "left" },
                            }

                        },
                        new RuleOption
                        {
                            Clauses = new List<Clause>
                            {
                                new Clause { ClauseType = ClauseType.Literal, Value = "right" },
                            }

                        }
                    }
                }
            }
            .ToDictionary(x => x.Name)
            ;

            var clause = new Clause
            {
                ClauseType = ClauseType.Reference,
                Value = "BASICTEST"
            };

            var regex = CUT.BuildClauseRegex(clause, rules);

            regex.Should().Be("(left)|(right)");

        }

        [TestMethod]
        public void ComplexReferenceTest()
        {

            var rules = new List<Rule>()
            {
                new Rule
                {
                    Name = "BASICTEST",
                    RuleType = RuleType.Token,
                    Options = new List<RuleOption>
                    {
                        new RuleOption
                        {
                            Clauses = new List<Clause>
                            {
                                new Clause { ClauseType = ClauseType.Literal, Value = "left" },
                            }

                        },
                        new RuleOption
                        {
                            Clauses = new List<Clause>
                            {
                                new Clause { ClauseType = ClauseType.Literal, Value = "right" },
                            }

                        }
                    }
                }
            }
            .ToDictionary(x => x.Name)
            ;

            var option = new RuleOption
            {
                Clauses = new List<Clause>
                {
                    new Clause{ ClauseType = ClauseType.Literal, Value = "structure" },
                    new Clause{ ClauseType = ClauseType.Reference, Value = "BASICTEST" }
                }
            };

            var regex = CUT.BuildOptionRegex(option, rules);

            regex.Should().Be("structure(left)|(right)");

        }

        [TestMethod]
        public void CaptureGroupTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.CaptureGroup,
                CaptureGroup = new CaptureGroup
                {
                    Clauses = new List<Clause>
                    {
                        new Clause{ ClauseType = ClauseType.Literal, Value= "honk" }
                    },
                    Modifier = CaptureModifier.None
                }
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("(honk)");

        }

        [TestMethod]
        public void CaptureGroupNoneToOneTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.CaptureGroup,
                CaptureGroup = new CaptureGroup
                {
                    Clauses = new List<Clause>
                    {
                        new Clause{ ClauseType = ClauseType.Literal, Value= "honk" }
                    },
                    Modifier = CaptureModifier.NoneToOne
                }
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("(honk)?");

        }

        [TestMethod]
        public void CaptureGroupOneOrMoreTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.CaptureGroup,
                CaptureGroup = new CaptureGroup
                {
                    Clauses = new List<Clause>
                    {
                        new Clause{ ClauseType = ClauseType.Literal, Value= "honk" }
                    },
                    Modifier = CaptureModifier.OneOrMore
                }
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("(honk)+");

        }

        [TestMethod]
        public void CaptureGroupOptionalTest()
        {

            var clause = new Clause
            {
                ClauseType = ClauseType.CaptureGroup,
                CaptureGroup = new CaptureGroup
                {
                    Clauses = new List<Clause>
                    {
                        new Clause{ ClauseType = ClauseType.Literal, Value= "honk" }
                    },
                    Modifier = CaptureModifier.Optional
                }
            };

            var regex = CUT.BuildClauseRegex(clause, null);

            regex.Should().Be("(honk)*");

        }

    }
}
