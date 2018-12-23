using System;
using System.Collections.Generic;

namespace PRTGService.Service
{
    public class StatsSummaryHolder
    {
        // private setter so no-one can change the dictionary itself
        // so create it in the constructor
        public IDictionary<string, List<float>> _stats { get; private set; }

        public StatsSummaryHolder()
        {
            _stats = new Dictionary<string, List<float>>();
        }


    }
}
