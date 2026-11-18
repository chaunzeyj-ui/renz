namespace PrintConnect.domain.Entities
{
    public class Inquiry
    {
        public int InquiryId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Use this to separate "Feedback", "Complaint", or "Inquiry"
        public string Type { get; set; } = string.Empty;

        // "Open" or "Resolved"
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}