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
    } 
}