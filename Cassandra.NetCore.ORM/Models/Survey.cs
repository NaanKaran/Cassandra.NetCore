using System;
using System.Collections.Generic;
using System.Text;
using Cassandra.NetCore.ORM.Attributes;

namespace Cassandra.NetCore.ORM.Models
{
    [CassandraTable("survey")]
    public class Survey
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string City { get; set; }
    }
}
