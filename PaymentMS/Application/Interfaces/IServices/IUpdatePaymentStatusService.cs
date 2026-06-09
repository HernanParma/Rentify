using Application.Dtos.Request;

namespace Application.Interfaces.IServices
{
    public interface IUpdatePaymentStatusService
    {
        Task<bool> UpdatePaymentStatus(UpdatePaymentStatusRequestDto updatePaymentStatusRequestDto);
    }
}
