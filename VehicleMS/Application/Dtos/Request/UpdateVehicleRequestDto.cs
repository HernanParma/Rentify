namespace Application.Dtos.Request;

public class UpdateVehicleRequestDto
{
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public string Plate { get; set; } = null!;
    public int VehicleStatusId { get; set; }
    public decimal PricePerDay { get; set; }
    public int BranchOfficeId { get; set; }
    public string Insurance { get; set; } = null!;
}
