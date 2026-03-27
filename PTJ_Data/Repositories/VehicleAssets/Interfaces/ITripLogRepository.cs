using Models.DTO.PurchaseProposal;
using Models.Models;

namespace Data.Repositories.VehicleAssets.Interfaces
{
    public interface ITripLogRepository
    {
        Task<TripLog> CreateAsync(TripLog trip);

        Task<TripLog?> GetRunningTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByVehicleIdAsync(int vehicleId);
        Task<TripLog?> GetLastCompletedTripByDriverIdAsync(int driverId);

        Task<TripLog?> GetByIdAsync(int tripId);

        Task UpdateAsync(TripLog trip);

        Task<List<TripLog>> GetTripHistoryByVehicleAsync(int? vehicleId);

        Task<List<Vehicle>> GetVehiclesDropAsync();
        Task<UserBasicDto?> GetDriverByVehicleIdAsync(int vehicleId);

        Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
        Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId);
        Task<List<TripLog>> GetRunningTripsByBranchAsync(int branchId);
        Task UpdateVehicleStatusAsync(int vehicleId, string status);
    }
}
