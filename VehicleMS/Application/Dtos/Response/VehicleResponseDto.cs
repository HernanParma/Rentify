namespace Application.Dtos.Response;

public class VehicleResponseDto
{
    public Guid VehicleId { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string Plate { get; set; } = null!;
    public int VehicleStatusId { get; set; }
    public string VehicleStatusName { get; set; } = null!;
    public decimal PricePerDay { get; set; }
    public int BranchOfficeId { get; set; }
    public string Insurance { get; set; } = null!;
}
