using Dev.Freela.Payments.API.Models;

namespace Dev.Freela.Payments.API.Services
{
    public class PaymentService : IPaymentService
    {
        public PaymentService()
        {

        }
        public Task<bool> Process(PaymentInfoInputModel paymentInfoInputModel)
        {
            return Task.FromResult(true);
        }
    }
}
