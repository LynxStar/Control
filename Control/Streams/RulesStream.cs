using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control.Streams
{
    public class RulesStream
    {

        public string Source { get; set; }
        public int Index { get; set; }

        public bool IsEndOfStream()
        {
            return Index == Source.Length;
        }

        public override string ToString()
        {

            if (IsEndOfStream())
            {
                return "@@@END_OF_STREAM@@@";
            }

            var past = String.Empty;
            var current = Source[Index];
            var preview = String.Empty;

            if (Index > 0)
            {
                var startFrom = 0;

                if (Index >= 10)
                {
                    startFrom = Index - 10;
                }
                past = Source.Substring(startFrom, Index - startFrom);

            }

            if (Index < Source.Length)
            {

                int remaining = Source.Length - Index;

                if (remaining >= 10)
                {
                    remaining = 10;
                }

                preview = Source.Substring(Index + 1, remaining);

            }

            return $"{past}⇃{current}⇂{preview}";
        }

    }
}
