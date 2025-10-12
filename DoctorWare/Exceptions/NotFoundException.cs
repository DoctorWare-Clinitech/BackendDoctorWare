using DoctorWare.Constants;

namespace DoctorWare.Exceptions
{
    public class NotFoundException : ApiException
    {
        public NotFoundException() : base(ErrorMessages.NOT_FOUND)
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
