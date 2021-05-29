﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Domain
{

    public class ConditionalOrExpression : Expression
    {
        public ConditionalAndExpression ConditionalAndExpression { get; set; }
        public List<ConditionalAndExpression> ConditionalAndExpressions { get; set; } = new List<ConditionalAndExpression>();
    }

    public class ConditionalAndExpression
    {
        public EqualityExpression EqualityExpression { get; set; }
        public List<EqualityExpression> EqualityExpressions { get; set; } = new List<EqualityExpression>();
    }

    public class EqualityExpression
    {
        public RelationalExpression Left { get; set; }
        public EqualityOperator EqualityOperator { get; set; }
        public RelationalExpression Right { get; set; }
    }

    public enum EqualityOperator
    {
        Equals,
        NotEquals
    }

    public class RelationalExpression
    {
        public AdditiveExpression Left { get; set; }
        public RelationalOperator RelationalOperator { get; set; }
        public AdditiveExpression Right { get; set; }
    }

    public enum RelationalOperator
    {
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public class AdditiveExpression
    {
        public MultiplicativeExpression MultiplicativeExpression { get; set; }
        public List<(AdditiveOperator op, MultiplicativeExpression expr)> MultiplicativeExpressions { get; set; }
            = new List<(AdditiveOperator op, MultiplicativeExpression expr)>();
    }

    public enum AdditiveOperator
    {
        Addition,
        Subtraction
    }

    public class MultiplicativeExpression
    {
        public UnaryExpression UnaryExpression { get; set; }
        public List<(MultiplicativeOperator op, UnaryExpression expr)> UnaryExpressions { get; set; }
            = new List<(MultiplicativeOperator op, UnaryExpression expr)>();
    }

    public enum MultiplicativeOperator
    {        
        Multiplication,
        Division
    }

}
