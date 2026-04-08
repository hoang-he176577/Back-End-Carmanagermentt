using Microsoft.AspNetCore.Mvc;
using Models.DTO.PurchaseProposal;
using Service.Services.Interfaces;

namespace API.Controllers
{
    [Route("api/post-purchase")]
    public class PostPurchaseController : BaseController
    {
        private readonly IPostPurchaseService _postPurchaseService;

        public PostPurchaseController(IPostPurchaseService postPurchaseService)
        {
            _postPurchaseService = postPurchaseService;
        }

        // POST: api/post-purchase/confirm-reception
        [HttpPost("confirm-reception")]
        public async Task<IActionResult> ConfirmReception([FromBody] AssetReceptionRequest request)
        {
            await _postPurchaseService.ConfirmReceptionAsync(request, GetUserId());
            return HandleSuccess("Asset reception confirmed. Waiting for payment approval.");
        }

        // POST: api/post-purchase/{id}/confirm-payment
        [HttpPost("{id}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            await _postPurchaseService.ConfirmPaymentAndActivateAssetAsync(id, GetUserId());
            return HandleSuccess("Payment approved successfully. Asset is now active in the system.");
        }
    }
}