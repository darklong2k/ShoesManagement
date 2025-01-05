using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class BlogImage
{
    public int BlogImageId { get; set; }

    public int? BlogId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Blog? Blog { get; set; }
}
