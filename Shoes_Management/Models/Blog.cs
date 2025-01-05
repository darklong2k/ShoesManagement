using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string? BlogContent { get; set; }

    public string? BlogTitle { get; set; }

    public string? BlogAuthor { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<BlogImage> BlogImages { get; set; } = new List<BlogImage>();
}
