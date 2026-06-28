using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context) { }

    public async Task<Category?> GetByNameAsync(string name) =>
        await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);
}
