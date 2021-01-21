using System;

namespace Cassandra.NetCore.ORM.Helpers
{
    public static class Mapper
    {
        public static T Map<T>(Row row)
        {
            var mapped = Activator.CreateInstance<T>();
            var properties = mapped.GetType().GetCassandraRelevantProperties();
            foreach (var property in properties)
            {
                var columnName = property.GetColumnNameMapping();
                var value = row[columnName];
                property.SetValue(mapped, value);
            }

            return mapped;
        }
    }
}
