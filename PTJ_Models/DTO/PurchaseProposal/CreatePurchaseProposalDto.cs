﻿
namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;

    public class CreatePurchaseProposalDto
    {

        [Required(ErrorMessage = "Mô tả đề xuất không được để trống.")]
        public string? Description { get; set; }

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
    }

}
