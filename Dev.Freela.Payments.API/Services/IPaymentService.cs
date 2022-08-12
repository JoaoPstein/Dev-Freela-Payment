using Dev.Freela.Payments.API.Models;

namespace Dev.Freela.Payments.API.Services
{
    public interface IPaymentService
    {
        Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel);
    }
}
