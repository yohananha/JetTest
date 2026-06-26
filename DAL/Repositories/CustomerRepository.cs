using JetTest.DAL.Interfaces;
using JetTest.Data;
using JetTest.Models;
using Microsoft.EntityFrameworkCore;

namespace JetTest.DAL.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email) =>
        await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
}
