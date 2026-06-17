namespace Application.Dtos.ProviderManagement
{
    public class CreateProviderRequestDto
    {
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; } // Hotel, Transport, Guide, Program
        public string? BusinessDescription { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? TaxNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class ProviderRequestResponse
    {
        public Guid? Id { get; set; }
        public string? BusinessName { get; set; }
        public string? BusinessType { get; set; }
        public string? Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ProviderEarningsResponse
    {
        public Guid? Id { get; set; }
        public decimal TotalEarnings { get; set; }
        public decimal PendingEarnings { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public int CompletedBookings { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class ApproveProviderRequestDto
    {
        public Guid? RequestId { get; set; }
    }

    public class RejectProviderRequestDto
    {
        public Guid? RequestId { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class ProviderBookingActionDto
    {
        public Guid? BookingId { get; set; }
    }

    public class ProviderContactRequestDto
    {
        public Guid? BookingId { get; set; }
        public string? Message { get; set; }
    }
}
