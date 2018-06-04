using System;
using System.Collections.Generic;

namespace Hotels
{
    public class SearchRequest
    {
        public Guid SearchId;
        public int VisitSourceId;
        public ISet<int> EstabIds;
    }
}