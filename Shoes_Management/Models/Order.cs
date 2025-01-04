using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string? Address { get; set; }

    public string? Status { get; set; }

    public string? Phone { get; set; }

    public string? Note { get; set; }

    public string? Reason { get; set; }

    public decimal? TotalAmount { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
