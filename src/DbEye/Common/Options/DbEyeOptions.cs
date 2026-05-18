using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbEye.Common.Options
{
    public class DbEyeOptions
    {
        public int SlowQueryThresholdMs { get; set; } = 500;
        public Dictionary<string, int> EndpointThresholds { get; set; } = [];
        public int NPlus1Threshold { get; set; } = 1;
        public Dictionary<string, int> EndpointNPlus1Thresholds { get; set; } = [];
        private HashSet<string> ExcludedEndpoints { get; set; } = [];
        internal IEnumerable<string> GetExcludedEndpoints() => ExcludedEndpoints;
        public List<string> AllowedEnvironments { get; set; } = ["Development"];

        public DbEyeOptions ExcludeEndpoints(params string[] endpoints)
        {
            foreach (var endpoint in endpoints)
                ExcludedEndpoints.Add(endpoint);
            
            return this;
        }

        public DbEyeOptions ExcludeScalar()
        {
            ExcludedEndpoints.Add("/scalar");
            ExcludedEndpoints.Add("/openapi");
            return this;
        }

        public DbEyeOptions ExcludeSwagger()
        {
            ExcludedEndpoints.Add("/swagger");
            return this;
        }
    } 
}