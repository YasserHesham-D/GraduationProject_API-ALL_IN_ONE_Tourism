using Application.Dtos.Programs;
using Application.Services.ProgramServices;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepo _programRepo;
        private readonly IProgramBookingRepo _bookingRepo;
        private readonly ILogger<ProgramService> _logger;

        public ProgramService(IProgramRepo programRepo, IProgramBookingRepo bookingRepo, ILogger<ProgramService> logger)
        {
            _programRepo = programRepo;
            _bookingRepo = bookingRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<ProgramResponse>> GetAllProgramsAsync()
        {
            var programs = await _programRepo.GetAllAsync();
            return programs.Select(MapToResponse);
        }

        public async Task<ProgramResponse?> GetProgramByIdAsync(Guid id)
        {
            var program = await _programRepo.GetByIdAsync(id);
            return program != null ? MapToResponse(program) : null;
        }

        public async Task<ProgramResponse> CreateProgramAsync(CreateProgramRequest request, string providerId)
        {
            var program = new Program
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                Category = request.Category,
                Location = request.Location,
                City = request.City,
                Country = request.Country,
                Price = request.Price,
                Duration = request.Duration,
                MaxParticipants = request.MaxParticipants,
                AvailableSpots = request.MaxParticipants,
                IncludedServices = request.IncludedServices,
                StartDate = request.StartDate,
                ProviderId = providerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProgram = await _programRepo.AddAsync(program);
            await _programRepo.SaveChangesAsync();
            return MapToResponse(createdProgram);
        }

        public async Task<ProgramBookingResponse> BookProgramAsync(Guid programId, BookProgramRequest request, string userId)
        {
            var program = await _programRepo.GetByIdAsync(programId);
            if (program == null)
                throw new Exception("Program not found");

            if (program.AvailableSpots < request.NumberOfParticipants)
                throw new Exception("Not enough available spots");

            var totalPrice = program.Price * request.NumberOfParticipants;

            var booking = new ProgramBooking
            {
                UserId = userId,
                ProgramId = programId,
                NumberOfParticipants = request.NumberOfParticipants,
                TotalPrice = totalPrice,
                Status = "pending",
                BookingDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return MapBookingToResponse(createdBooking, program.Name);
        }

        public async Task<IEnumerable<ProgramBookingResponse>> GetUserProgramBookingsAsync(string userId)
        {
            var bookings = await _bookingRepo.GetUserBookingsAsync(userId);
            return bookings.Select(b => MapBookingToResponse(b, b.Program?.Name ?? "Unknown"));
        }

        public async Task<IEnumerable<ProgramBookingResponse>> GetProviderProgramBookingsAsync(string providerId)
        {
            var bookings = await _bookingRepo.GetProviderBookingsAsync(providerId);
            return bookings.Select(b => MapBookingToResponse(b, b.Program?.Name ?? "Unknown"));
        }

        private ProgramResponse MapToResponse(Program program)
        {
            return new ProgramResponse
            {
                Id = program.Id,
                Name = program.Name,
                Description = program.Description,
                ImageUrl = program.ImageUrl,
                Category = program.Category,
                Location = program.Location,
                City = program.City,
                Country = program.Country,
                Price = program.Price,
                Duration = program.Duration,
                MaxParticipants = program.MaxParticipants,
                AvailableSpots = program.AvailableSpots,
                IncludedServices = program.IncludedServices,
                Rating = program.Rating,
                ReviewCount = program.ReviewCount,
                StartDate = program.StartDate
            };
        }

        private ProgramBookingResponse MapBookingToResponse(ProgramBooking booking, string programName)
        {
            return new ProgramBookingResponse
            {
                Id = booking.Id,
                ProgramId = booking.ProgramId,
                ProgramName = programName,
                NumberOfParticipants = booking.NumberOfParticipants,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate
            };
        }
    }
}
