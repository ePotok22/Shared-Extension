using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace FFF.Shared
{
    //
    // Summary:
    //     Serves as the base class for application-defined exceptions.
    [Serializable]
    [ComVisible(true)]
    public class BusinessException : Exception
    {
        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class.
        public BusinessException()
            : base("Arg_BusinessException")
        {
        }

        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with a specified
        //     error message.
        //
        // Parameters:
        //   message:
        //     A message that describes the error.
        public BusinessException(string message)
            : base(message) { }

        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with a specified
        //     error message and a reference to the inner exception that is the cause of this
        //     exception.
        //
        // Parameters:
        //   message:
        //     The error message that explains the reason for the exception.
        //
        //   innerException:
        //     The exception that is the cause of the current exception. If the innerException
        //     parameter is not a null reference, the current exception is raised in a catch
        //     block that handles the inner exception.
        public BusinessException(string message, Exception innerException)
            : base(message, innerException) { }

        //
        // Summary:
        //     Initializes a new instance of the System.ApplicationException class with serialized
        //     data.
        //
        // Parameters:
        //   info:
        //     The object that holds the serialized object data.
        //
        //   context:
        //     The contextual information about the source or destination.
        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
