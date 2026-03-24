using System;

namespace Models.Models
{
    public partial class VehicleReceptionRecord
    {
        public const string ReceivedPendingPaymentStatus = "Received_Pending_Payment";
        public const string RejectedStatus = "Rejected";
        public const string CompletedStatus = "Completed";

        public int Id { get; set; }
        public int PurchaseProposalId { get; set; }
        public int BranchId { get; set; }
        public string? LicensePlate { get; set; }
        public string Status { get; set; } = ReceivedPendingPaymentStatus;
        public string? Notes { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual PurchaseProposal? PurchaseProposal { get; set; }
        public virtual Branch? Branch { get; set; }

        public void Complete()
        {
            Status = CompletedStatus;
            UpdatedAt = DateTime.Now;
        }

        public void Reject(string? reason)
        {
            Status = RejectedStatus;
            Notes = reason;
            UpdatedAt = DateTime.Now;
        }
    }
}
