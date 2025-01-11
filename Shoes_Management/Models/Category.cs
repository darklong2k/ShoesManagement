using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shoes_Management.Models;

public partial class Category
{
    
    public int CategoryId { get; set; }

    public int? ParentId { get; set; }
    [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
    [StringLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự.")]
    public string? Name { get; set; }
    [StringLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
    public string? Description { get; set; }

    public bool Status { get; set; }
   
    [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug chỉ chứa các ký tự chữ thường, số, và dấu gạch ngang.")]
    [StringLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự.")]
    public string? slug {  get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
