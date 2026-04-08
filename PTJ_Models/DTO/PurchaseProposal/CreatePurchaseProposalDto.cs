
namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;

    public class CreatePurchaseProposalDto
    {

        [Required(ErrorMessage = "Mô tả đề xuất không được để trống.")]
        public string? Description { get; set; }

        public DateTime? CompletionDeadline { get; set; }

        public List<CreateBulkPurchaseDetailDto> Details { get; set; } = new();
    }
    public class CreateBulkPurchaseDetailDto
    {
        [Range(1, 10000, ErrorMessage = "Số lượng không hợp lệ.")]
        public int Quantity { get; set; }
        public string? Description { get; set; }

        [Range(0, 9999999999999, ErrorMessage = "Đơn giá vượt quá giới hạn cho phép.")]
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }

        [Range(1, 100, ErrorMessage = "Số chỗ ngồi phải lớn hơn 0.")]
        public int? Seats { get; set; }

        [Required(ErrorMessage = "Nhãn hiệu xe không được để trống.")]
        [StringLength(100, ErrorMessage = "Nhãn hiệu không được vượt quá 100 ký tự.")]
        public string? Manufacturer { get; set; }

        public string? AcquisitionMethod { get; set; }

        [Range(0, 9999999999999, ErrorMessage = "Thuế đăng ký vượt quá giới hạn cho phép.")]
        public decimal? RegistrationTax { get; set; }

        [Range(0, 9999999999999, ErrorMessage = "Phí bảo trì đường bộ vượt quá giới hạn cho phép.")]
        public decimal? RoadMaintenanceFee { get; set; }

        [Range(0, 9999999999999, ErrorMessage = "Phí biển số xe vượt quá giới hạn cho phép.")]
        public decimal? LicensePlateFee { get; set; }

        [Range(0, 9999999999999, ErrorMessage = "Phí bảo hiểm vượt quá giới hạn cho phép.")]
        public decimal? InsuranceFee { get; set; }

        public bool? HasCamera158 { get; set; }

        public bool? HasGsht { get; set; }

        public string? Version { get; set; }
    }

}
