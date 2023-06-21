using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports.GarmentExportBankReceiptJournal
{
    public interface IGarmentExportBankReceiptJournalService
    {
        List<GarmentExportBankReceiptJournalViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset);
        MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset);
    }
}
