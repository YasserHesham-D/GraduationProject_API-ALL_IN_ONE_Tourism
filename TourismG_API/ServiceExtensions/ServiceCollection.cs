
using Application.Services;
using Application.Services.AccountServices;
using Application.Services.GuideServices;
using Application.Services.HotelServices;
using Application.Services.ProgramServices;
using Application.Services.ProviderServices;
using Application.Services.TransportServices;
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

            // Account Services
            services.AddScoped<IAccountsRepo, AccountsRepo>();
            services.AddScoped<IAccountServices, AccountServices>();

            // Hotel Services
            services.AddScoped<IHotelRepo, HotelRepo>();
            services.AddScoped<IHotelBookingRepo, HotelBookingRepo>();
            services.AddScoped<IHotelService, HotelService>();

            // Guide Services
            services.AddScoped<IGuideRepo, GuideRepo>();
            services.AddScoped<IGuideBookingRepo, GuideBookingRepo>();
            services.AddScoped<IGuideService, GuideService>();

            // Program Services
            services.AddScoped<IProgramRepo, ProgramRepo>();
            services.AddScoped<IProgramBookingRepo, ProgramBookingRepo>();
            services.AddScoped<IProgramService, ProgramService>();

            // Transport Services
            services.AddScoped<ITransportRepo, TransportRepo>();
            services.AddScoped<ITransportBookingRepo, TransportBookingRepo>();
            services.AddScoped<ITransportService, TransportService>();

            // Provider Services
            services.AddScoped<IProviderRequestRepo, ProviderRequestRepo>();
            services.AddScoped<IProviderEarningsRepo, ProviderEarningsRepo>();
            services.AddScoped<IProviderService, ProviderService>();

            //services.AddScoped<SeedDataService>();

            return services;
        }
    }
}
