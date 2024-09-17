using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.GarmentFinance.Adjustment;
using Com.Ambassador.Service.Finance.Accounting.Lib.Models.GarmentFinance.Adjustment;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.Ambassador.Service.Finance.Accounting.Test.DataUtils.GarmentFinance.Adjustment
{
    public class GarmentFinanceAdjustmentDataUtil
    {
        private readonly GarmentFinanceAdjustmentService Service;

        public GarmentFinanceAdjustmentDataUtil(GarmentFinanceAdjustmentService service)
        {
            Service = service;
        }

        public GarmentFinanceAdjustmentModel GetNewData()
        {
            return new GarmentFinanceAdjustmentModel
            {
                AdjustmentNo = "no",
                Remark = "remarks",
                Date = DateTimeOffset.Now,
                GarmentCurrencyCode = "number",
                GarmentCurrencyId = 1,
                GarmentCurrencyRate = 1,
                IsUsed=false,
                Items = new List<GarmentFinanceAdjustmentItemModel>
                {
                    new GarmentFinanceAdjustmentItemModel()
                    {
                        COAId = 1,
                        COACode = "code",
                        COAName = "name",
                        Credit = 0,
                        Debit = 1,

                    },
                    new GarmentFinanceAdjustmentItemModel()
                    {
                        COAId = 1,
                        COACode = "code",
                        COAName = "name",
                        Credit = 1,
                        Debit = 0,
                    },
                }
            };
        }

        public async Task<GarmentFinanceAdjustmentModel> GetTestData()
        {
            GarmentFinanceAdjustmentModel model = GetNewData();
            await Service.CreateAsync(model);
            return await Service.ReadByIdAsync(model.Id);
        }
    }
}
