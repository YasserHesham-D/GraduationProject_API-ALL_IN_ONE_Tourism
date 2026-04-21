using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.DbContext;
using Infrastructure.Repository;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Repositories.ModelRepo
{
    public class AccountsRepo : Repository<User>, IAccountsRepo
    {

        public AccountsRepo(AppDbContext context, ILogger<User> logger) : base(context, logger)
        {
        }
    }
}
