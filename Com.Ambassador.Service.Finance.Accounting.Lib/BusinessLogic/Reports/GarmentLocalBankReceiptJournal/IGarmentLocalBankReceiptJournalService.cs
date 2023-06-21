using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports.GarmentLocalBankReceiptJournal
{
    public interface IGarmentLocalBankReceiptJournalService
    {
        List<GarmentLocalBankReceiptJournalViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset);
        MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
