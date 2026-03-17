using Models.DTO.PurchaseProposal;
using Models.DTO.Vehicles;


namespace Service.Services.VehicleAssets.Interfaces
{
    public interface ITripLogService
    {
        Task<int> StartTripAsync(StartTripRequestDto dto);

        Task EndTripAsync(int tripId, EndTripRequestDto dto);

        Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId);

        Task<List<ManageVehicleTripDto>> GetManageVehiclesAsync(int branchId, string? tab);
        Task<List<ListVehicleDrop>> GetVehicleDropAsync();
        Task<UserBasicDto> GetDriverByVehicleIdAsync(int vehicleId);
    }
}
