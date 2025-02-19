using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.VBRequestDocument;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.VBRealizationDocument
{
    public class FormDto : IValidatableObject
    {
        public DateTimeOffset? Date { get; set; }
        public string Type { get; set; }
        public VBRequestDocumentDto VBRequestDocument { get; set;}
        public UnitDto SuppliantUnit { get; set; }
        public CurrencyDto Currency { get; set; }
        public List<FormItemDto> Items { get; set; }
        public int Id { get; set; }
        public bool IsInklaring { get; set; }
        public string Remark { get; set; }
        public string ReasonForDelay { get; set; }
        public bool IsDelay { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Date == null)
                yield return new ValidationResult("Tanggal harus diisi", new List<string> { "Date" });

            if (IsDelay && Type == "Dengan Nomor VB")
            {
                if (string.IsNullOrWhiteSpace(ReasonForDelay))
                {
                    yield return new ValidationResult("Alasan Keterlambatan harus diisi", new List<string> { "ReasonForDelay" });
                }
            }

            if (string.IsNullOrWhiteSpace(Type))
                yield return new ValidationResult("Jenis VB harus diisi", new List<string> { "Type" });
            else
            {
                if (Type == "Tanpa Nomor VB")
                {
                    if (SuppliantUnit == null || SuppliantUnit.Id.GetValueOrDefault() <= 0)
                        yield return new ValidationResult("Unit harus diisi", new List<string> { "SuppliantUnit" });

                    if (Currency == null || Currency.Id.GetValueOrDefault() <= 0)
                        yield return new ValidationResult("Mata Uang harus diisi", new List<string> { "Currency" });
                }
                else
                {
                    if (VBRequestDocument == null || VBRequestDocument.Id.GetValueOrDefault() <= 0)
                        yield return new ValidationResult("VB harus diisi", new List<string> { "VBRequestDocument" });
                }
            }

            if (Items == null || Items.Count.Equals(0))
            {
                yield return new ValidationResult("Nomor SPB harus diisi", new List<string> { "Item" });
            }
            else if (Items.Count > 0)
            {
                int CountItemsError = 0;
                string ItemsError = "[";
                List<string> spbNo = new List<string>();
                foreach (var item in Items)
                {
                    ItemsError += "{ ";
                    
                    if (item.UnitPaymentOrder == null || item.UnitPaymentOrder.Id.GetValueOrDefault() <= 0)
                    {
                        CountItemsError++;
                        ItemsError += "'UnitPaymentOrder': 'SPB harus diisi', ";
                    }
                    if (spbNo.Count == 0)
                    {
                        spbNo.Add(item.UnitPaymentOrder.No);
                    }
                    else if (spbNo.Contains(item.UnitPaymentOrder.No))
                    {
                        CountItemsError++;
                        ItemsError += "'UnitPaymentOrder': 'No SPB duplikat', ";
                    }

                    ItemsError += "}, ";
                }

                ItemsError += "]";

                if (CountItemsError > 0)
                    yield return new ValidationResult(ItemsError, new List<string> { "Items" });
            }
        }
    }
}