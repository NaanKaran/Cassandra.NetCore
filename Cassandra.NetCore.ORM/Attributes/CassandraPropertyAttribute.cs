using System;

namespace Cassandra.NetCore.ORM.Attributes
{
    public class CassandraPropertyAttribute : Attribute
    {
        public CassandraPropertyAttribute()
        {
        }

        public CassandraPropertyAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; }
    }

    public class PrimaryKey : Attribute
    {
        public PrimaryKey()
        {
        }

        public PrimaryKey(string attributeName)
        {
            AttributeName = attributeName;
        }

        public string AttributeName { get; }
    }
}
