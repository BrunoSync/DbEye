using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Models;

namespace DbEye.Core.Collector
{
    public class DbEyeCollector
    {
        private List<QueryModel> _queries = new();
        public IReadOnlyCollection<QueryModel> Queries => _queries;

        public void Add(QueryModel query)
        => _queries.Add(query);
    }
}