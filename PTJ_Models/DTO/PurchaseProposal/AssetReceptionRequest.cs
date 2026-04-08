using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.PurchaseProposal;

public class AssetReceptionRequest
{
    public int ProposalId { get; set; } 
    public string LicensePlate { get; set; } = string.Empty; 
    public string? Description { get; set; } 
}