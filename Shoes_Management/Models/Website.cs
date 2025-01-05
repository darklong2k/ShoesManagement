using System;
using System.Collections.Generic;

namespace Shoes_Management.Models;

public partial class Website
{
    public int WebsiteId { get; set; }

    public string? WebsiteName { get; set; }

    public string? WebsiteAddress { get; set; }

    public string? WebsiteLinkFb { get; set; }

    public string? WebsiteImage { get; set; }

    public string? WebsitePhone { get; set; }

    public string? WebsiteEmail { get; set; }
}
