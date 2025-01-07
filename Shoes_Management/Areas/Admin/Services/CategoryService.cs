using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shoes_Management.Models;
using System.Threading.Tasks;


namespace Shoes_Management.Areas.Admin.Services
{   
    public class CategoryService : ICategoryService
    {
        private readonly Shoescontext _context;

        public CategoryService(Shoescontext context)
        {
            _context = context;
        }
        

        public async Task<bool> AddCategoryAsync(Category model)
        {
            var category = new Category
            {
                Name = model.Name,
                ParentId = model.ParentId,
                Description = model.Description,
                Status = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateCategoryAsync(Category model)
        {
            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null) return false;

            category.Name = model.Name;
            category.ParentId = model.ParentId;
            category.Description = model.Description;
            category.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeactivateCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;

            category.Status = false;
            category.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
