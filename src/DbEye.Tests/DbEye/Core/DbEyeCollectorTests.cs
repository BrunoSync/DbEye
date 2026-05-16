using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Models;
using DbEye.Core.Collector;
using FluentAssertions;
using Xunit;

namespace DbEye.Tests.DbEye.Core
{
    public class DbEyeCollectorTests
    {
        [Fact]
        [Trait("Core", "Collector")]
        public void Queries_should_be_empty_on_initialization()
        {
            var collector = new DbEyeCollector();

            collector.Queries.Should().BeEmpty();
        }

        [Fact]
        [Trait("Core", "Collector")]
        public void Add_should_add_query_to_collection()
        {
            var query = CreateQuery;

            var collector = new DbEyeCollector();

            collector.Add(query);

            collector.Queries.Should().HaveCount(1);
            collector.Queries.Should().Contain(query);
        }

        [Fact]
        [Trait("Core", "Collector")]
        public void Add_multiple_queries_should_all_be_present()
        {
            var query1 = CreateQuery;
            var query2 = CreateQuery;
            var query3 = CreateQuery;

            var collector = new DbEyeCollector();

            collector.Add(query1);
            collector.Add(query2);
            collector.Add(query3);

            collector.Queries.Should().HaveCount(3);
        }

        public QueryModel CreateQuery
        => new QueryModel(
            "SELECT * FROM users",
            TimeSpan.FromMilliseconds(Random.Shared.Next(30, 500))
        );
    }
}