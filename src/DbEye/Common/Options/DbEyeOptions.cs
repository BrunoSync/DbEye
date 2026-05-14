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
        public HashSet<string> ExcludedEndpoints { get; set; } = [];

        public DbEyeOptions ExcludeEndpoints(params string[] endpoints)
        {
            foreach (var endpoint in ExcludedEndpoints)
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