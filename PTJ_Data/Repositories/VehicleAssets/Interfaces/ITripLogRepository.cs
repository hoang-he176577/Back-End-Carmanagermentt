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

        Task<TripLog?> GetRunningTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByDriverIdAsync(int driverId);
        Task<List<TripLog>> GetLastCompletedTripsByVehicleIdsAsync(List<int> vehicleIds);

        Task<bool> HasActiveTripAsync(int vehicleId);

        Task AddAsync(TripLog trip);

        Task SaveChangesAsync();
    }
}
