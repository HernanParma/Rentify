using Application.Dtos.Request;
using Application.Dtos.Response;

namespace Application.Interfaces.IServices;

public interface IBranchOfficeService
{
    Task<IEnumerable<BranchOfficeResponseDto>> GetAllAsync();
    Task<BranchOfficeResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<BranchOfficeMapResponseDto>> GetMapAsync();
    Task<BranchOfficeResponseDto> CreateAsync(CreateBranchOfficeRequestDto request);
    Task<BranchOfficeResponseDto?> UpdateAsync(int id, UpdateBranchOfficeRequestDto request);
    Task<bool> DeleteAsync(int id);
}
