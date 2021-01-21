using System;

namespace Cassandra.NetCore.ORM.Attributes
{
    public class CassandraTableAttribute : Attribute
    {
        public CassandraTableAttribute()
        {
        }

        public CassandraTableAttribute(string tableName)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}
