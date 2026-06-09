using Application.Dtos.Request;
using FluentValidation;

namespace Application.Validators
{
    public class PaymentRequestValidator : AbstractValidator<CreatePaymentRequestDto>
    {
        public PaymentRequestValidator()
        {
            RuleFor(x => x.ReservationId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.PaymentMethodId).GreaterThan(0);
        }
    }
}
