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

        public PrimaryKey(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }

    public class ClusterKey : Attribute
    {
        public ClusterKey()
        {
        }

        public ClusterKey(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }

    public class IndexKey : Attribute
    {
        public IndexKey()
        {
        }

        public IndexKey(int order)
        {
            Order = order;
        }

        public int Order { get; }
    }
}
