using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.VehicleAssets.Interfaces
{
    public interface ITripLogRepository
    {
        Task<List<TripLog>> GetAllAsync();

        Task<TripLog?> GetByIdAsync(int id);

        Task<bool> HasActiveTripAsync(int vehicleId);

        Task AddAsync(TripLog trip);

        Task SaveChangesAsync();
    }
}
