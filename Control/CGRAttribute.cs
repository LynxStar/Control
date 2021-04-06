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

}
