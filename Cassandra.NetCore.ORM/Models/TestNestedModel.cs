using System;
using System.Collections.Generic;
using System.Text;
using Cassandra.NetCore.ORM.Attributes;

namespace Cassandra.NetCore.ORM.Models
{
    [CassandraTable("NestedTable")]
   public class TestNestedModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public TestChildTable ChildTable { get; set; }

    }
}
