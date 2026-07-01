using IBS.Models.Enums;

namespace IBS.Utility.Helpers
{
    public class SubAccountHelper
    {
        public static (SubAccountType? Type, int? Id) DetermineCvSubAccount(
            int? customerId,
            int? supplierId,
            int? bankId,
            int? companyId,
            int? employeeId)
        {
            if (customerId.HasValue)
            {
                return (SubAccountType.Customer, customerId.Value);
            }

            if (supplierId.HasValue)
            {
                return (SubAccountType.Supplier, supplierId.Value);
            }

            if (bankId.HasValue)
            {
                return (SubAccountType.BankAccount, bankId.Value);
            }

            if (companyId.HasValue)
            {
                return (SubAccountType.Company, companyId.Value);
            }

            if (employeeId.HasValue)
            {
                return (SubAccountType.Employee, employeeId.Value);
            }

            return (null, null);
        }
    }
}
