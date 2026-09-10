using System.ComponentModel.DataAnnotations;
using IBS.Models.Enums;

namespace IBS.Models.Filpride.MasterFile
{
    public sealed record CustomerRequestPayload(
        string CustomerName, string CustomerAddress, string CustomerTin, string? BusinessStyle,
        string CustomerTerms, string CustomerType, string VatType, bool WithHoldingVat,
        bool WithHoldingTax, ClusterArea? ClusterCode, string? StationCode, decimal CreditLimit,
        decimal CreditLimitAsOfToday, string? ZipCode, decimal? RetentionRate, bool HasMultipleTerms,
        string Type, bool RequiresPriceAdjustment, int? CommissioneeId, decimal CommissionRate,
        decimal CwtPercent, decimal CwVatPercent);

    public sealed record CustomerBranchRequestPayload(
        int CustomerId, string BranchName, string BranchAddress, string BranchTin);

    public sealed record SupplierRequestPayload(
        string SupplierName, string SupplierAddress, string SupplierTin, string SupplierTerms,
        string VatType, string TaxType, string Category, string? EmployeeNumber, string? TradeName,
        string? Branch, string? DefaultExpenseNumber, decimal? WithholdingTaxPercent,
        string? WithholdingTaxTitle, string? ReasonOfExemption, string? Validity,
        DateTime? ValidityDate, string? ZipCode, bool RequiresPriceAdjustment,
        string? ProofOfRegistrationFilePath, string? ProofOfRegistrationFileName,
        string? ProofOfExemptionFilePath, string? ProofOfExemptionFileName);

    public sealed record BankAccountRequestPayload(string Bank, string Branch, string AccountNo, string AccountName);

    public sealed record ServiceRequestPayload(string Name, int CurrentAndPreviousId, int UnearnedId, int Percent);

    public sealed record ChartOfAccountRequestPayload(
        int ParentAccountId,
        [param: StringLength(200)] string AccountName);

    public sealed record PickupPointRequestPayload(string Depot, int SupplierId);
}
