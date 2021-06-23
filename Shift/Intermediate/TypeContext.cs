using Shift.Domain;
using Shift.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shift.Intermediate
{
    public class TypeContext
    {

        public Application Application { get; init; } = new Application();
        public TypeService TypeService { get; init; } = new TypeService(this);
        public TypeTracker Tracker { get; init; }

        public TypeContext()
        {
            Tracker = new TypeTracker(Application, TypeService);
        }

    }
}
