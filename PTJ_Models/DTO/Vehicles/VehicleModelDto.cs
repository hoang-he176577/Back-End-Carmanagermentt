namespace Models.DTO.Vehicles;

public class VehicleModelDto
{
    public int Id { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int? Seats { get; set; }
    public string? EngineType { get; set; }
    public decimal? DefaultPrice { get; set; }
}
