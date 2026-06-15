namespace Application.Dtos.ProviderManagement
{
    public class CreateProviderRequestDto
    {
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty; // Hotel, Transport, Guide, Program
        public string BusinessDescription { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string DocumentUrl { get; set; } = string.Empty;
    }

    public class ProviderRequestResponse
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ProviderEarningsResponse
    {
        public Guid Id { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public int CompletedBookings { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ApproveProviderRequestDto
    {
        public Guid RequestId { get; set; }
    }

    public class RejectProviderRequestDto
    {
        public Guid RequestId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class ProviderBookingActionDto
    {
        public Guid BookingId { get; set; }
    }

    public class ProviderContactRequestDto
    {
        public Guid BookingId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
