using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? SupplierId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public decimal? RatingAvg { get; set; }

    public string? Slug { get; set; }

    public string? Image { get; set; }

    public bool? Outstanding { get; set; }

    public int? Views { get; set; }

    public int? Likes { get; set; }

    public string? Status { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
