using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class CartDetail
{
    public int CustomerId { get; set; }

    public int ProductDetailId { get; set; }

    public int? Quantity { get; set; }

    public decimal? TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ProductDetail ProductDetail { get; set; } = null!;
}
