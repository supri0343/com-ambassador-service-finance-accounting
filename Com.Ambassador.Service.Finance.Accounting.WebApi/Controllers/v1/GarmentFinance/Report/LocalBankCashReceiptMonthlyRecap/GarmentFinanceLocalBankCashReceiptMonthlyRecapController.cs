﻿using AutoMapper;
using Com.Ambassador.Service.Finance.Accounting.Lib.BusinessLogic.GarmentFinance.Reports.LocalBankCashReceiptMonthlyRecap;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.IdentityService;
using Com.Ambassador.Service.Finance.Accounting.Lib.Services.ValidateService;
using Com.Ambassador.Service.Finance.Accounting.WebApi.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.Ambassador.Service.Finance.Accounting.WebApi.Controllers.v1.GarmentFinance.Report.LocalBankCashReceiptMonthlyRecap
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/bank-cash-receipts/local-monthly-recap")]
    public class GarmentFinanceLocalBankCashReceiptMonthlyRecapController : Controller
    {
        private IIdentityService IdentityService;
        private readonly IValidateService ValidateService;
        private readonly IGarmentFinanceLocalBankCashReceiptMonthlyRecapService Service;
        private readonly string ApiVersion;
        private readonly IMapper Mapper;

        public GarmentFinanceLocalBankCashReceiptMonthlyRecapController(IIdentityService identityService, IValidateService validateService, IGarmentFinanceLocalBankCashReceiptMonthlyRecapService service, IMapper mapper)
        {
            IdentityService = identityService;
            ValidateService = validateService;
            Service = service;
            ApiVersion = "1.0.0";
            Mapper = mapper;
        }


        protected void VerifyUser()
        {
            IdentityService.Username = User.Claims.ToArray().SingleOrDefault(p => p.Type.Equals("username")).Value;
            IdentityService.Token = Request.Headers["Authorization"].FirstOrDefault().Replace("Bearer ", "");
            IdentityService.TimezoneOffset = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
        }

        [HttpGet("monitoring")]
        public IActionResult Get([FromQuery] DateTimeOffset? dateFrom, [FromQuery] DateTimeOffset? dateTo)
        {
            try
            {
                VerifyUser();
                int offSet = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                //int offSet = 7;
                var data = Service.GetMonitoring(dateFrom, dateTo, offSet);

                return Ok(new
                {
                    apiVersion = ApiVersion,
                    data,
                    message = General.OK_MESSAGE,
                    statusCode = General.OK_STATUS_CODE
                });
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                   new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                   .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetXls([FromQuery] DateTimeOffset? dateFrom, [FromQuery] DateTimeOffset? dateTo)
        {
            try
            {
                VerifyUser();
                int offSet = Convert.ToInt32(Request.Headers["x-timezone-offset"]);
                var xls = Service.GenerateExcel(dateFrom, dateTo, offSet);

                string filename = String.Format("Rekap Memo per Bulan - Lokal - {0}.xlsx", DateTime.UtcNow.ToString("ddMMyyyy"));

                return File(xls.Item1.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", xls.Item2);
            }
            catch (Exception e)
            {
                Dictionary<string, object> Result =
                    new ResultFormatter(ApiVersion, General.INTERNAL_ERROR_STATUS_CODE, e.Message)
                    .Fail();
                return StatusCode(General.INTERNAL_ERROR_STATUS_CODE, Result);
            }
        }
    }
}