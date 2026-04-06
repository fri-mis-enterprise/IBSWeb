namespace IBS.DTOs
{
    public class FilprideCollectionReceiptCsvForDcrDto
    {
        public string DATE { get; set; } = null!;
        public string PAYEE { get; set; } = null!;
        public string CVNO { get; set; } = null!;
        public string CHECKNO { get; set; } = null!;
        public string PARTICULARS { get; set; } = null!;
        public decimal AMOUNT { get; set; }
        public string ACCOUNTNO { get; set; } = null!;
        public string CHECKDATE { get; set; } = null!;
        public bool ISORCANCEL { get; set; }
        public string DATEDEPOSITED { get; set; } = null!;
    }

    public class FilprideCheckVoucherHeaderCsvForDcrDto
    {
        public string VOUCHER_NO { get; set; } = null!;
        public string VCH_DATE { get; set; } = null!;
        public string PAYEE { get; set; } = null!;
        public decimal AMOUNT { get; set; }
        public string PARTICULARS { get; set; } = null!;
        public string CHECKNO { get; set; } = null!;
        public string CHKDATE { get; set; } = null!;
        public string ACCOUNTNO { get; set; } = null!;
        public string CASHPODATE { get; set; } = null!;
        public string DCRDATE { get; set; } = null!;
        public bool ISCANCELLED { get; set; }
    }

    public class FilprideCheckVoucherDetailsCsvForDcrDto
    {
        public string ACCTCD { get; set; } = null!;
        public string ACCTNAME { get; set; } = null!;
        public string CVNO { get; set; } = null!;
        public decimal DEBIT { get; set; }
        public decimal CREDIT { get; set; }
        public string CUSTOMER_NAME { get; set; } = null!;
        public string BANK { get; set; } = null!;
        public string EMPLOYEE_NAME { get; set; } = null!;
        public string COMPANY_NAME { get; set; } = null!;
    }

    public class FilprideCollectionDetailsCsvForDcrDto
    {
        public string ACCTCD { get; set; } = null!;
        public string ACCTNAME { get; set; } = null!;
        public string CRNO { get; set; } = null!;
        public decimal DEBIT { get; set; }
        public decimal CREDIT { get; set; }
        public string CUSTOMER_NAME { get; set; } = null!;
        public string BANK { get; set; } = null!;
    }
}
