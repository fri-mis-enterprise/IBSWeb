using IBS.Models.Enums;

namespace IBS.Models.Filpride.Books
{
    public static class FilprideGeneralLedgerBookExtensions
    {
        public static void SetCounterparty(
            this IEnumerable<FilprideGeneralLedgerBook> entries,
            CounterpartyType? counterpartyType,
            int? counterpartyId,
            string? counterpartyName)
        {
            foreach (var entry in entries)
            {
                entry.CounterpartyType = counterpartyType;
                entry.CounterpartyId = counterpartyId;
                entry.CounterpartyName = counterpartyName;
            }
        }
    }
}
