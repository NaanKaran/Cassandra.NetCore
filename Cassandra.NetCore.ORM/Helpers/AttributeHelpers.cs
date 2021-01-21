using System;
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
                return property.Name + " " + GetOperand(property.PropertyType) + " PRIMARY KEY";
            }

            return property.Name + " " + GetOperand(property.PropertyType);
        }

        private static string GetOperand(Type nodeType)
        {

            var typeCode = Type.GetTypeCode(nodeType);
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
                default:
                    throw new NotSupportedException($"Node type {nodeType} is a valid operand");
            }
        }
    }
}
