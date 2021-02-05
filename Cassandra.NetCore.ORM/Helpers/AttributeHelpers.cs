using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cassandra.NetCore.ORM.Attributes;
using Cassandra.NetCore.ORM.Exceptions;

namespace Cassandra.NetCore.ORM.Helpers
{
    public static class AttributeHelpers
    {
        public static string ExtractTableName<T>(this Type type)
        {
            var tableNameAttribute = typeof(T).GetCustomAttributes(typeof(CassandraTableAttribute), true).FirstOrDefault() as CassandraTableAttribute;
            if (tableNameAttribute != null)
                return tableNameAttribute.TableName;

            throw new MissingTableAttributeException(typeof(T));
        }

        public static PropertyInfo[] GetCassandraRelevantProperties(this Type type)
        {

            var properties = type.GetProperties()
                                 .Where(p => !p.GetCustomAttributes(false)
                                     .OfType<CassandraIgnoreAttribute>().Any());

            return properties.ToArray();
        }

        public static string GetColumnNameMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            if (cassandraPropertyAttribute != null)
                return cassandraPropertyAttribute.AttributeName;

            return property.Name;
        }


        public static string GetColumnNameAndPrimaryKeyMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<PrimaryKey>();
            if (cassandraPropertyAttribute != null)
            {
                return property.Name + " " + GetOperand(property) + " PRIMARY KEY";
            }

            return property.Name + " " + GetOperand(property);
        }

        public static (string,int) GetPrimaryKeyColumnsMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            var primaryKey = property.GetCustomAttribute<PrimaryKey>();
            

            if (cassandraPropertyAttribute != null && primaryKey != null)
                return (cassandraPropertyAttribute.AttributeName, primaryKey.Order);
           
            if (primaryKey != null)
            {
                return (property.Name,primaryKey.Order);
            }

            return (null,0);
        }


        public static (string, int) GetClusterKeyColumnsMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            var clusterKey = property.GetCustomAttribute<ClusterKey>();


            if (cassandraPropertyAttribute != null && clusterKey != null)
                return (cassandraPropertyAttribute.AttributeName, clusterKey.Order);

            if (clusterKey != null)
            {
                return (property.Name, clusterKey.Order);
            }

            return (null, 0);
        }

        public static (string, int) GetIndexKeyColumnsMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            var indexKey = property.GetCustomAttribute<IndexKey>();


            if (cassandraPropertyAttribute != null && indexKey != null)
                return (cassandraPropertyAttribute.AttributeName, indexKey.Order);

            if (indexKey != null)
            {
                return (property.Name, indexKey.Order);
            }

            return (null, 0);
        }

        public static string GetColumnNameAndTypeMapping(this PropertyInfo property)
        {
            var cassandraPropertyAttribute = property.GetCustomAttribute<CassandraPropertyAttribute>();
            if (cassandraPropertyAttribute != null)
                return cassandraPropertyAttribute.AttributeName + " " + GetOperand(property);

            return property.Name + " " + GetOperand(property);
        }

        private static string GetOperand(PropertyInfo propertyInfo)
        {

            var typeCode = Type.GetTypeCode(propertyInfo.PropertyType);
            switch (typeCode)
            {
               
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return "int";
                case TypeCode.String:
                    return "text";
                case TypeCode.DateTime:
                    return "timestamp";
                case TypeCode.Object:
                    return GetObjectType(propertyInfo);
                default:
                    throw new NotSupportedException($"Node type {propertyInfo.PropertyType} is a valid operand");
            }
        }

        private static string GetObjectType(PropertyInfo propertyInfo)
        {
            var isEnumerable = propertyInfo.PropertyType.IsSubclassOf(typeof(IEnumerable));



            if (propertyInfo.PropertyType.IsClass)
            {
                var tt = propertyInfo.PropertyType;
            }

            if (isEnumerable)
            {

                return $"list<FROZEN<{propertyInfo.PropertyType.Name}>>";
            }

            return $"FROZEN<{propertyInfo.PropertyType.Name}>";
        }


    }
}
