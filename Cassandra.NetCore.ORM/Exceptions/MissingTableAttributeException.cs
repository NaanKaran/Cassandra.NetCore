using System;

namespace Cassandra.NetCore.ORM.Exceptions
{
    public class MissingTableAttributeException : Exception
    {
        public MissingTableAttributeException()
        {
        }

        public MissingTableAttributeException(Type classType)
            : base($"Missing table attribute for class {classType}")
        { 
        }
    }


    public class MissingPrimaryKeyAttributeException : Exception
    {
        public MissingPrimaryKeyAttributeException()
        {
        }

        public MissingPrimaryKeyAttributeException(Type classType)
            : base($"Missing PrimaryKey attribute for class {classType}")
        {
        }
    }
}
