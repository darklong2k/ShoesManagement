using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Review
{
    public int ProductDetailId { get; set; }

    public int CustomerId { get; set; }

    public string? Comment { get; set; }

    public byte? Rating { get; set; }

    public DateTime? ReviewDate { get; set; }

    public string? Status { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ProductDetail ProductDetail { get; set; } = null!;
}
