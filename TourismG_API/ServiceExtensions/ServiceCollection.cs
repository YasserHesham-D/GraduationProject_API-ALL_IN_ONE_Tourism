
using Application.Services.AccountServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Interfaces.IRepository;
using Domain.Interfaces.IUnitOfWork;
using Infrastructure.Repositories.ModelRepo;
using Infrastructure.Repository;
using Infrastructure.UnitOfWork;

namespace Presentation.ServiceExtensions
{
    public static class ServiceCollection
    {
        public static IServiceCollection ServicesCollection(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepo<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
   

            services.AddScoped<IAccountsRepo, AccountsRepo>();
            services.AddScoped<IAccountServices, AccountServices>();


            services.AddScoped<SeedDataService>();

            return services;
        }
    }
}
