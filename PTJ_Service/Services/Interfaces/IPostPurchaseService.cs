using Models.DTO.PurchaseProposal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Interfaces
{
    public interface IPostPurchaseService
    {
        
        Task ConfirmReceptionAsync(AssetReceptionRequest request, int operatorId);

        
        Task ConfirmPaymentAndActivateAssetAsync(int proposalId, int accountantId);
    }
}
