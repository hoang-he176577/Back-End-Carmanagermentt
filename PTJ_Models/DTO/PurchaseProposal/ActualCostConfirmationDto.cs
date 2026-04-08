namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;

    public class ActualCostConfirmationDto
    {
        [Required(ErrorMessage = "Vui lòng nhập chi phí thực tế.")]
        [Range(0, 9999999999999, ErrorMessage = "Chi phí thực tế vượt quá giới hạn.")]
        public decimal ActualCost { get; set; }

        public string? AccountantNote { get; set; }

        public List<DetailCostUpdateDto>? DetailUpdates { get; set; }
    }

    public class DetailCostUpdateDto
    {
        public int DetailId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RegistrationTax { get; set; }
        public decimal RoadMaintenanceFee { get; set; }
        public decimal LicensePlateFee { get; set; }
        public decimal InsuranceFee { get; set; }
    }
}
