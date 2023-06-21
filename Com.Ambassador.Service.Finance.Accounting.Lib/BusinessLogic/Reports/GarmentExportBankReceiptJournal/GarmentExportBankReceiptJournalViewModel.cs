using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports.GarmentExportBankReceiptJournal
{
    public class GarmentExportBankReceiptJournalViewModel
    {
        public string Remark { get; set; }
        public string Account { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }

    public class GarmentExportBankReceiptJournalTempViewModel
    {        
        public string CreditCoaCode { get; set; }
        public string CreditCoaName { get; set; }
        public string ReceiptNo { get; set; }
        public DateTimeOffset ReceiptDate { get; set; }
        public decimal Amount { get; set; }
    }
}
