using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace DTOMaker.Runtime
{
    public interface IMutability
    {
        /// <summary>
        /// Returns true if the graph cannot be modified.
        /// </summary>
        bool IsFrozen();
    }
}
