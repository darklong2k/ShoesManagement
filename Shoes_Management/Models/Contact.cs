using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string? ContactName { get; set; }

    public string? ContactContent { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactStatus { get; set; }
}
