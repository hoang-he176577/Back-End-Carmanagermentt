using System;

namespace Models.DTO.PurchaseProposal;

public class CreatePurchaseProposalRequestDto
{
    public int VehicleModelId { get; set; }
    public int Quantity { get; set; }
    public decimal EstimatedPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
}
