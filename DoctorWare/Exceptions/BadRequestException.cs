using DoctorWare.Constants;

namespace DoctorWare.Exceptions
{
    public class BadRequestException : ApiException
    {
        public BadRequestException() : base(ErrorMessages.BAD_REQUEST)
        {
        }

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
