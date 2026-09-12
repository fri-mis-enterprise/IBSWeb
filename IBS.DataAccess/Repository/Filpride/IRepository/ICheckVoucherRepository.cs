using IBS.DataAccess.Repository.IRepository;
using IBS.Models.Filpride.AccountsPayable;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface ICheckVoucherRepository : IRepository<FilprideCheckVoucherHeader>
    {
        Task<string> GenerateCodeAsync(string type, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeMultipleInvoiceAsync(string type, CancellationToken cancellationToken = default);

        Task<string> GenerateCodeMultiplePaymentAsync(string type, CancellationToken cancellationToken = default);

        Task UpdateInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default);

        Task UpdateMultipleInvoicingVoucher(decimal paymentAmount, int invoiceVoucherId, CancellationToken cancellationToken = default);

        Task PostAsync(FilprideCheckVoucherHeader header, IEnumerable<FilprideCheckVoucherDetail> details, CancellationToken cancellationToken = default);
    }
}
