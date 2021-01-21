namespace Cassandra.NetCore.ORM.Models
{
    internal class Query
    {
        public Query()
        {
        }

        public Query(string statement, object[] values)
        {
            Statement = statement;
            Values = values;
        }

        public string Statement { get; set; }
        public object[] Values { get; set; }
    }
}
