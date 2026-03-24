using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AccessoryGoodsReceiptDetail
{
    public int Id { get; set; }

    public int ReceiptId { get; set; }

    public int AccessoryId { get; set; }

    public int ReceivedQuantity { get; set; }

    public decimal? ActualUnitPrice { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual AccessoryGoodsReceipt Receipt { get; set; } = null!;
}
