namespace DoctorWare.DTOs.Response
{
    public enum ResendConfirmationStatus
    {
        Sent,
        AlreadyConfirmed,
        CooldownActive,
        NotFound
    }

    public sealed class ResendConfirmationResult
    {
        private ResendConfirmationResult(ResendConfirmationStatus status)
        {
            Status = status;
        }

        public ResendConfirmationStatus Status { get; }

        public static ResendConfirmationResult Sent() => new ResendConfirmationResult(ResendConfirmationStatus.Sent);

        public static ResendConfirmationResult AlreadyConfirmed() => new ResendConfirmationResult(ResendConfirmationStatus.AlreadyConfirmed);

        public static ResendConfirmationResult CooldownActive() => new ResendConfirmationResult(ResendConfirmationStatus.CooldownActive);

        public static ResendConfirmationResult NotFound() => new ResendConfirmationResult(ResendConfirmationStatus.NotFound);
    }
}

