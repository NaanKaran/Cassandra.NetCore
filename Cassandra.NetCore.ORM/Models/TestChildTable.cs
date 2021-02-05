using System;
using System.Collections.Generic;
using System.Text;
using Cassandra.NetCore.ORM.Attributes;

namespace Cassandra.NetCore.ORM.Models
{
    [CassandraTable("NestedChildTable")]
    public class TestChildTable
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
