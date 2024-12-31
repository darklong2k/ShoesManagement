using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class ProductDetail
{
    public int ProductDetailId { get; set; }

    public int? SizeId { get; set; }

    public int? ColorId { get; set; }

    public int? ProductId { get; set; }

    public int? StockQuantity { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual Color? Color { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product? Product { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Size? Size { get; set; }
}
