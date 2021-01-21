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
}
