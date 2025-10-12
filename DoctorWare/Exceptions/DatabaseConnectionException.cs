using DoctorWare.Constants;

namespace DoctorWare.Exceptions
{
    public class DatabaseConnectionException : ApiException
    {
        public DatabaseConnectionException(string message) : base(message)
        {
        }

        public DatabaseConnectionException() : base(ErrorMessages.DB_CONNECTION_ERROR)
        {
        }

        public DatabaseConnectionException(Exception innerException) : base(ErrorMessages.DB_CONNECTION_ERROR, innerException)
        {
        }

        public DatabaseConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
