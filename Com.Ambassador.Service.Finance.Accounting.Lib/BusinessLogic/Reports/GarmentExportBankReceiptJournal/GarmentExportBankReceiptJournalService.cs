using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.HttpClientService;
using Newtonsoft.Json;
using Com.Ambassador.Service.Finance.Accounting.Lib.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.IdentityService;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Com.Ambassador.Service.Finance.Accounting.Lib.Models.GarmentFinance.BankCashReceiptDetail;
using Com.Ambassador.Service.Finance.Accounting.Lib.Models.GarmentFinance.BankCashReceipt;

namespace Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports.GarmentExportBankReceiptJournal
{
    public class GarmentExportBankReceiptJournalService : IGarmentExportBankReceiptJournalService
    {
        private readonly IServiceProvider _serviceProvider;
        private const string UserAgent = "finance-service";
        private readonly FinanceDbContext _dbContext;
        private readonly IIdentityService _identityService;

        protected DbSet<BankCashReceiptItemModel> DbSetBankCashReceiptItem;
        protected DbSet<BankCashReceiptModel> DbSetBankCashReceipt;

        public GarmentExportBankReceiptJournalService(IServiceProvider serviceProvider, FinanceDbContext dbContext)
        {
            _dbContext = dbContext;

            _identityService = serviceProvider.GetService<IIdentityService>();
            _serviceProvider = serviceProvider;
        }

        public List<GarmentExportBankReceiptJournalViewModel> GetReportData(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            var Query = GetReportQuery(dateFrom, dateTo, offset);
            return Query;
        }

        public List<GarmentExportBankReceiptJournalViewModel> GetReportQuery(DateTime? dateFrom, DateTime? dateTo, int offset)

        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            List<GarmentExportBankReceiptJournalViewModel> data = new List<GarmentExportBankReceiptJournalViewModel>();
            //List<GarmentExportBankReceiptJournalTempViewModel> data1 = new List<GarmentExportBankReceiptJournalTempViewModel>();
        
            var Query = from a in _dbContext.GarmentFinanceBankCashReceipts
                        where a.ReceiptDate.AddHours(7).Date >= DateFrom.Date && a.ReceiptDate.AddHours(7).Date <= DateTo.Date
                        && a.CurrencyRate > 1

                        group new { TotalAmount = a.Amount * a.CurrencyRate } by new
                        {
                            a.DebitCoaCode,
                            a.DebitCoaName
                        } into G

                        select new GarmentExportBankReceiptJournalViewModel
                        {
                            Remark = G.Key.DebitCoaCode,
                            Account = G.Key.DebitCoaName,
                            Debit = Math.Round(G.Sum(m => m.TotalAmount), 2),
                            Credit = 0
                        };

            var Query1 = from a in _dbContext.GarmentFinanceBankCashReceipts
                        where a.ReceiptDate.AddHours(7).Date >= DateFrom.Date && a.ReceiptDate.AddHours(7).Date <= DateTo.Date
                        && a.CurrencyRate > 1

                        select new GarmentExportBankReceiptJournalTempViewModel
                        {
                           CreditCoaCode = a.BankCashReceiptTypeCoaCode,
                           CreditCoaName = a.BankCashReceiptTypeCoaName,
                           ReceiptDate = a.ReceiptDate,
                           ReceiptNo = a.ReceiptNo,
                           Amount = a.Amount * a.CurrencyRate
                        };

            var join = from a in Query1
                       select new GarmentExportBankReceiptJournalViewModel
                       {
                           Remark = "PIUTANG USAHA EXPORT GARMENT",
                           Credit = a.Amount,
                           Debit = 0,
                           Account = "1103.00.5.00"
                       };

            foreach (var item in Query)
            {
                var debit = new GarmentExportBankReceiptJournalViewModel
                {
                    Remark = item.Remark,
                    Account = item.Account,
                    Debit = item.Debit,
                    Credit = 0
                };

                data.Add(debit);
            }

            var credit = new GarmentExportBankReceiptJournalViewModel
            {
                Remark = "       PIUTANG USAHA EXPORT GARMENT",
                Account = "1103.00.5.00",
                Debit = 0,
                Credit = join.Sum(a => a.Credit),
            };
            data.Add(credit);

            var total = new GarmentExportBankReceiptJournalViewModel
            {
                Remark = " ",
                Account = "JUMLAH",
                Debit = join.Sum(a => a.Credit),
                Credit = join.Sum(a => a.Credit),
            };
           
            data.Add(total);
            
            return data;
        }

        public MemoryStream GenerateExcel(DateTime? dateFrom, DateTime? dateTo, int offset)
        {
            DateTime DateFrom = dateFrom == null ? new DateTime(1970, 1, 1) : (DateTime)dateFrom;
            DateTime DateTo = dateTo == null ? DateTime.Now : (DateTime)dateTo;

            var Query = GetReportQuery(dateFrom, dateTo, offset);
            DataTable result = new DataTable();

            result.Columns.Add(new DataColumn() { ColumnName = "AKUN DAN KETERANGAN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "AKUN", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "DEBET", DataType = typeof(string) });
            result.Columns.Add(new DataColumn() { ColumnName = "KREDIT", DataType = typeof(string) });

            ExcelPackage package = new ExcelPackage();

            if (Query.ToArray().Count() == 0)
            {
                result.Rows.Add("", "", 0, 0);
                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);

                    sheet.Column(1).Width = 50;
                    sheet.Column(2).Width = 15;
                    sheet.Column(3).Width = 20;
                    sheet.Column(4).Width = 20;

                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT AMBASSADOR GARMINDO";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "IKHTISAR JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PENERIMAAN KAS BANK GARMENT EXPORT";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("dd-MM-yyyy") + " S/D " + DateTo.ToString("dd-MM-yyyy");
                    sheet.Cells[$"D6"].Style.Font.Bold = true;

                    #endregion
                    sheet.Cells["A8"].LoadFromDataTable(item.Key, true, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.None);

                    //sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                }
            }
            else
            {
                int index = 0;
                foreach (var d in Query)
                {
                    index++;

                    result.Rows.Add(d.Remark, d.Account, d.Debit, d.Credit);
                }

                bool styling = true;

                foreach (KeyValuePair<DataTable, String> item in new List<KeyValuePair<DataTable, string>>() { new KeyValuePair<DataTable, string>(result, "Territory") })
                {
                    var sheet = package.Workbook.Worksheets.Add(item.Value);

                    #region KopTable
                    sheet.Cells[$"A1:D1"].Value = "PT AMBASSADOR GARMINDO";
                    sheet.Cells[$"A1:D1"].Merge = true;
                    sheet.Cells[$"A1:D1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A1:D1"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A1:D1"].Style.Font.Bold = true;

                    sheet.Cells[$"A2:D2"].Value = "ACCOUNTING DEPT.";
                    sheet.Cells[$"A2:D2"].Merge = true;
                    sheet.Cells[$"A2:D2"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                    sheet.Cells[$"A2:D2"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A2:D2"].Style.Font.Bold = true;

                    sheet.Cells[$"A4:D4"].Value = "IKHTISAR JURNAL";
                    sheet.Cells[$"A4:D4"].Merge = true;
                    sheet.Cells[$"A4:D4"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    sheet.Cells[$"A4:D4"].Style.Font.Bold = true;

                    sheet.Cells[$"C5"].Value = "BUKU HARIAN";
                    sheet.Cells[$"C5"].Style.Font.Bold = true;
                    sheet.Cells[$"D5"].Value = ": PENERIMAAN KAS BANK GARMENT EXPORT";
                    sheet.Cells[$"D5"].Style.Font.Bold = true;

                    sheet.Cells[$"C6"].Value = "PERIODE";
                    sheet.Cells[$"C6"].Style.Font.Bold = true;
                    sheet.Cells[$"D6"].Value = ": " + DateFrom.ToString("dd-MM-yyyy") + " S/D " + DateTo.ToString("dd-MM-yyyy");
                    sheet.Cells[$"D6"].Style.Font.Bold = true;

                    #endregion
                    sheet.Cells["A8"].LoadFromDataTable(item.Key, true, (styling == true) ? OfficeOpenXml.Table.TableStyles.Light16 : OfficeOpenXml.Table.TableStyles.None);

                    //sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                }
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);

            return stream;
        }
    }
}
