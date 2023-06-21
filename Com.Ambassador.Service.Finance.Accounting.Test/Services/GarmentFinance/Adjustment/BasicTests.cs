using Com.Ambassador.Service.Finance.Accounting.Lib;
using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.GarmentFinance.Adjustment;
using Com.Ambassador.Service.Finance.Accounting.Lib.Models.GarmentFinance.Adjustment;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.HttpClientService;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Ambassador.Service.Finance.Accounting.Lib.ViewModels.GarmentFinance.Adjustment;
using Com.Ambassador.Service.Finance.Accounting.Test.DataUtils.GarmentFinance.Adjustment;
using Com.Ambassador.Service.Finance.Accounting.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.Ambassador.Service.Finance.Accounting.Test.Services.GarmentFinance.Adjustment
{
    public class BasicTests
    {
        private const string ENTITY = "GarmentFinanceAdjustments";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private FinanceDbContext _dbContext(string testName)
        {
            DbContextOptionsBuilder<FinanceDbContext> optionsBuilder = new DbContextOptionsBuilder<FinanceDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            FinanceDbContext dbContext = new FinanceDbContext(optionsBuilder.Options);

            return dbContext;
        }


        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            serviceProvider
                .Setup(x => x.GetService(typeof(IIdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test", TimezoneOffset = 7 });


            return serviceProvider;
        }

        private GarmentFinanceAdjustmentDataUtil _dataUtil(GarmentFinanceAdjustmentService service, string testname)
        {
            return new GarmentFinanceAdjustmentDataUtil(service);
        }

        [Fact]
        public async Task Should_Success_Create_Data()
        {
            GarmentFinanceAdjustmentService service = new GarmentFinanceAdjustmentService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            GarmentFinanceAdjustmentModel model = _dataUtil(service, GetCurrentMethod()).GetNewData();
            var Response = await service.CreateAsync(model);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public async Task Should_Success_Get_Data()
        {
            GarmentFinanceAdjustmentService service = new GarmentFinanceAdjustmentService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var data = await _dataUtil(service, GetCurrentMethod()).GetTestData();
            var Response = service.Read(1, 25, "{}", null, null, "{}");
            Assert.NotEmpty(Response.Data);
        }

        [Fact]
        public async Task Should_Success_Get_Data_By_Id()
        {
            GarmentFinanceAdjustmentService service = new GarmentFinanceAdjustmentService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            GarmentFinanceAdjustmentModel model = await _dataUtil(service, GetCurrentMethod()).GetTestData();
            var Response = await service.ReadByIdAsync(model.Id);
            Assert.NotNull(Response);
        }

        [Fact]
        public async Task Should_Success_Delete_Data()
        {
            GarmentFinanceAdjustmentService service = new GarmentFinanceAdjustmentService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));


            GarmentFinanceAdjustmentModel model = await _dataUtil(service, GetCurrentMethod()).GetTestData();
            var newModel = await service.ReadByIdAsync(model.Id);

            var Response = await service.DeleteAsync(newModel.Id);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public async Task Should_Success_Update_Data()
        {
            GarmentFinanceAdjustmentService service = new GarmentFinanceAdjustmentService(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));


            GarmentFinanceAdjustmentModel model = await _dataUtil(service, GetCurrentMethod()).GetTestData();
            var newModel = await service.ReadByIdAsync(model.Id);
            //newModel.BGCheckNumber = "newBG";
            var Response1 = await service.UpdateAsync(newModel.Id, newModel);
            Assert.NotEqual(0, Response1);

            GarmentFinanceAdjustmentModel model2 = await _dataUtil(service, GetCurrentMethod()).GetTestData();
            //var newModel2 = await service.ReadByIdAsync(model.Id);
            GarmentFinanceAdjustmentModel newModel2 = new GarmentFinanceAdjustmentModel();
            newModel2.Id = model2.Id;
            newModel2.Items = new List<GarmentFinanceAdjustmentItemModel> { model2.Items.First() };
            newModel2.Items.First().Credit = 100;
            newModel2.Items.First().Debit = 100;
            var Response = await service.UpdateAsync(model2.Id, newModel2);
            Assert.NotEqual(0, Response);

            GarmentFinanceAdjustmentItemModel newItem = new GarmentFinanceAdjustmentItemModel
            {
                COAId = 3,
                COACode = "no3",
                COAName = "Name",
                Debit = 3,
                Credit=0
            };

            GarmentFinanceAdjustmentItemModel newItem2 = new GarmentFinanceAdjustmentItemModel
            {
                COAId = 3,
                COACode = "no3",
                COAName = "Name",
                Credit = 3,
                Debit=0
            };

            newModel2.Items.Add(newItem);
            newModel2.Items.Add(newItem2);
            var Response3 = await service.UpdateAsync(model2.Id, newModel2);
            Assert.NotEqual(0, Response);
        }

        [Fact]
        public void Should_Success_Validate_All_Null_Data()
        {
            GarmentFinanceAdjustmentViewModel vm = new GarmentFinanceAdjustmentViewModel();

            Assert.True(vm.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Null_Invoice_Data()
        {
            GarmentFinanceAdjustmentViewModel vm = new GarmentFinanceAdjustmentViewModel();
            vm.Items = new List<GarmentFinanceAdjustmentItemViewModel>
            {
                new GarmentFinanceAdjustmentItemViewModel()
                {
                    COA=null,
                    Debit=0,
                    Credit=1
                }
            };

            Assert.True(vm.Validate(null).Count() > 0);
        }

    }
}

