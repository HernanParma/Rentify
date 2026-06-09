namespace Application.Dtos.Response;

public class BranchOfficeMapResponseDto
{
    public int BranchOfficeId { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Hours { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsActive { get; set; }
    public int AvailableVehicleCount { get; set; }
}
