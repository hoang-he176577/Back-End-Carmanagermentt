using Models.DTO.PurchaseProposal;
using Models.DTO.Vehicles;
using Models.DTO.VehicleDistribution;

namespace Service.Services.VehicleAssets.Interfaces
{
    public interface ITripLogService
    {
        Task<int> StartTripAsync(StartTripRequestDto dto);

        Task EndTripAsync(int tripId, EndTripRequestDto dto);

        Task<List<TripHistoryResponseDto>> GetAllTripHistoryAsync();

        Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId);

        Task<List<ManageVehicleTripDto>> GetManageVehiclesAsync(int branchId, string? tab);
        Task<List<ListVehicleDrop>> GetVehicleDropAsync();
        Task<UserBasicDto> GetDriverByVehicleIdAsync(int vehicleId);

        Task<List<PendingTransferDto>> GetPendingTransfersAsync(int branchId);
        Task<List<PendingTransferDto>> GetInTransitTransfersAsync(int branchId);
    }
}
