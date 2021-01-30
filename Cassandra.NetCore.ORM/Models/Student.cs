using System;
using System.Collections.Generic;
using System.Text;
using Cassandra.NetCore.ORM.Attributes;

namespace Cassandra.NetCore.ORM.Models
{
    [CassandraTable("Student")]
    public class Student
    {
        [PrimaryKey(1)]
        public int  Id { get; set; }

        [PrimaryKey(2)]
        [IndexKey(1)]
        public string  Standard { get; set; }

        public string  Section { get; set; }
        public DateTime  CreatedOn { get; set; }

    }
}
