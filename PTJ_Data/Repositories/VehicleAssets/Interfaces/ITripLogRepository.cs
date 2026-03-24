using Models.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Data.Repositories.VehicleAssets.Interfaces
{
    public interface ITripLogRepository
    {
        Task<List<TripLog>> GetAllAsync();
        Task<TripLog?> GetByIdAsync(int id);

        Task<TripLog?> GetRunningTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByDriverIdAsync(int driverId);
        Task<List<TripLog>> GetLastCompletedTripsByVehicleIdsAsync(List<int> vehicleIds);

        Task<List<TripLog>> GetTripHistoryByVehicleAsync(int vehicleId);
        Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
        Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId);
        Task<List<TripLog>> GetRunningTripsByBranchAsync(int branchId);

        Task<bool> HasActiveTripAsync(int vehicleId);
        Task AddAsync(TripLog trip);
        Task UpdateAsync(TripLog trip);
        Task SaveChangesAsync();
    }
}
