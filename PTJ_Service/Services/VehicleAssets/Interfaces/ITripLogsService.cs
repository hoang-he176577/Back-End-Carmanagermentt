using Models.DTO.Vehicles;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Interfaces
{
    public interface ITripLogsService
    {
        Task<List<TripLog>> GetAllAsync();

        Task<TripLog?> GetByIdAsync(int id);

        Task<bool> StartTripAsync(StartTripRequestDto request);

        Task<bool> EndTripAsync(int tripId, EndTripRequestDto request);

        Task<List<ManageVehicleTripDto>> GetManageVehiclesAsync(int branchId, string? tab);

        Task<List<TripHistoryResponseDto>> GetAllTripHistoryAsync();

        Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId);
    }
}
