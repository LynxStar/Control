using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control
{
    
    public class Direct : CGRAttribute
    {

        public Direct()
        {
            MapDirect = true;
        }

        public Direct(int innerPosition) : this()
        {
            InnerPosition = innerPosition;
        }

    }

    public class CGRAttribute : Attribute
    {

        public int Position { get; init; } = 0;
        public bool MapDirect { get; init; } = false;
        public int InnerPosition { get; init; } = 0;


    }

    public interface IOption
    {
        public object Value { get; set; }
    }
    public class Option<T1> : IOption
    {
        public object Value { get; set; }

    }
    public class Option<T1, T2> : IOption
    {
        public object Value { get; set; }

    }
    public class Option<T1, T2, T3> : IOption
    {
        public object Value { get; set; }
    }
    public class Option<T1, T2, T3, T4> : IOption
    {
        public object Value { get; set; }
    }
    public class Option<T1, T2, T3, T4, T5> : IOption
    {
        public object Value { get; set; }
    }

    public class TokenValue
    {
        public string Token { get; set; }
        public string Value { get; set; }
    }

    public class Instance : Attribute
    {

        public int Position { get; init; } = 0;

        public Instance(int position)
        {
            Position = position;
        }

    }


}
