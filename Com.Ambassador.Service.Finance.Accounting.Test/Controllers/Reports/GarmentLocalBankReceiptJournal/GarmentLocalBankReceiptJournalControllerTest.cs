using AutoMapper;
using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports;
using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.Reports.GarmentLocalBankReceiptJournal;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.ValidateService;
using Com.Ambassador.Service.Finance.Accounting.Lib.Utilities;
using Com.Ambassador.Service.Finance.Accounting.WebApi.Controllers.v1.Reports.GarmentLocalBankReceiptJournal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Com.Ambassador.Service.Finance.Accounting.Test.Controllers.Reports.GarmentLocalBankReceiptJournal
{
    public class GarmentLocalBankReceiptJournalControllerTest
    {
        protected GarmentLocalBankReceiptJournalController GetController((Mock<IIdentityService> IdentityService, Mock<IValidateService> ValidateService, Mock<IGarmentLocalBankReceiptJournalService> Service, Mock<IMapper> Mapper) mocks)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            GarmentLocalBankReceiptJournalController controller = new GarmentLocalBankReceiptJournalController(mocks.IdentityService.Object, mocks.ValidateService.Object, mocks.Service.Object, mocks.Mapper.Object);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            return controller;
        }

        public (Mock<IIdentityService> IdentityService, Mock<IValidateService> ValidateService, Mock<IGarmentLocalBankReceiptJournalService> Service, Mock<IMapper> Mapper) GetMocks()
        {
            return (IdentityService: new Mock<IIdentityService>(), ValidateService: new Mock<IValidateService>(), Service: new Mock<IGarmentLocalBankReceiptJournalService>(), Mapper: new Mock<IMapper>());
        }

        Mock<IServiceProvider> GetServiceProvider()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IIdentityService)))
              .Returns(new IdentityService() { TimezoneOffset = 1, Token = "token", Username = "username" });

            var validateService = new Mock<IValidateService>();
            serviceProvider
              .Setup(s => s.GetService(typeof(IValidateService)))
              .Returns(validateService.Object);
            return serviceProvider;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }
     
        protected ServiceValidationException GetServiceValidationException(dynamic dto)
        {
            var serviceProvider = new Mock<IServiceProvider>();
            var validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(dto, serviceProvider.Object, null);
            return new ServiceValidationException(validationContext, validationResults);
        }

        [Fact]
        public void GetMonitoring_Success()
        {
            var serviceMock = new Mock<IGarmentLocalBankReceiptJournalService>();
            serviceMock.Setup(s => s.GetReportData(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new List<GarmentLocalBankReceiptJournalViewModel>());
        
            var service = serviceMock.Object;
            var identityProviderMock = new Mock<IServiceProvider>();
            var identityProvider = identityProviderMock.Object;

            var controller = GetController(GetMocks());
            var response = controller.Get(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Get_Exception_InternalServerError()
        {
            var serviceMock = new Mock<IGarmentLocalBankReceiptJournalService>();
            serviceMock.Setup(s => s.GetReportData(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Throws(new Exception());
          
            var service = serviceMock.Object;
            var identityProviderMock = new Mock<IServiceProvider>();
            var identityProvider = identityProviderMock.Object;

            var controller = GetController(GetMocks());
            var response = controller.GetXls(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_GetXls()
        {
            var serviceMock = new Mock<IGarmentLocalBankReceiptJournalService>();
            serviceMock.Setup(s => s.GenerateExcel(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MemoryStream());

            var service = serviceMock.Object;
            var identityProviderMock = new Mock<IServiceProvider>();
            var identityProvider = identityProviderMock.Object;

            var controller = GetController(GetMocks());
            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/xls";
            var response = controller.GetXls(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.NotNull(response);
        }
     
        [Fact]
        public void Should_Error_GetXls()
        {
            var serviceMock = new Mock<IGarmentLocalBankReceiptJournalService>();
            serviceMock.Setup(s => s.GenerateExcel(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MemoryStream());

            var service = serviceMock.Object;
            var identityProviderMock = new Mock<IServiceProvider>();
            var identityProvider = identityProviderMock.Object;

            var controller = GetController(GetMocks());
            controller.ControllerContext.HttpContext.Request.Headers["Accept"] = "application/xls";
            var response = controller.GetXls(null, null);

            Assert.NotNull(response);
        }
    }
}
