using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shoes_Management.Models;
namespace Shoes_Management.Areas.Admin.Services
{
    public interface ICategoryService
    {
        Task<bool> AddCategoryAsync(Category model);
        Task<bool> UpdateCategoryAsync(Category model);
        Task<bool> DeactivateCategoryAsync(int id);

    }
}
