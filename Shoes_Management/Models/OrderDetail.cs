using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }

    public int ProductDetailId { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ProductDetail ProductDetail { get; set; } = null!;
}
