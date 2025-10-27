namespace DoctorWare.DTOs.Response
{
    public enum EmailConfirmationStatus
    {
        Success,
        AlreadyConfirmed,
        InvalidToken,
        Expired,
        NotFound
    }

    public sealed class EmailConfirmationResult
    {
        private EmailConfirmationResult(EmailConfirmationStatus status)
        {
            Status = status;
        }

        public EmailConfirmationStatus Status { get; }

        public static EmailConfirmationResult Success() => new EmailConfirmationResult(EmailConfirmationStatus.Success);

        public static EmailConfirmationResult AlreadyConfirmed() => new EmailConfirmationResult(EmailConfirmationStatus.AlreadyConfirmed);

        public static EmailConfirmationResult InvalidToken() => new EmailConfirmationResult(EmailConfirmationStatus.InvalidToken);

        public static EmailConfirmationResult Expired() => new EmailConfirmationResult(EmailConfirmationStatus.Expired);

        public static EmailConfirmationResult NotFound() => new EmailConfirmationResult(EmailConfirmationStatus.NotFound);
    }
}

