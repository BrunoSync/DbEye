using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbEye.Common.Models
{
    public sealed record QueryModel
    (   
        string Sql,
        TimeSpan DurationInMs
    );
}