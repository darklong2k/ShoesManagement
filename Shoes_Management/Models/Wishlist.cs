using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Wishlist
{
    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
