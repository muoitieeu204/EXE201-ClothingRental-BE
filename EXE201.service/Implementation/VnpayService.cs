using EXE201.Service.DTOs;
using EXE201.Service.Interface;
using EXE201.Service.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Implementation
{
    public class VnpayService : IVnpayService
    {
        private readonly VnPayLibrary _vnPayLibrary;
        private readonly IConfiguration _configuration;

        public VnpayService(VnPayLibrary vnPayLibrary, IConfiguration configuration)
        {
            _vnPayLibrary = vnPayLibrary;
            _configuration = configuration;
        }

        public string CreatePaymentURL(OrderInfoDTO order, HttpContext httpContext)
        {
            var vnpaySettings = _configuration.GetSection("VnPaySettings");

            _vnPayLibrary.AddRequestData("vnp_Version", VnPayLibrary.VERSION); 
            _vnPayLibrary.AddRequestData("vnp_Command", "pay");
            _vnPayLibrary.AddRequestData("vnp_TmnCode", vnpaySettings["TmnCode"]);
            _vnPayLibrary.AddRequestData("vnp_Amount", order.Amount.ToString()); 
            _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
            _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
            _vnPayLibrary.AddRequestData("vnp_OrderType", "other");

            _vnPayLibrary.AddRequestData("vnp_OrderInfo", order.OrderDesc);
            _vnPayLibrary.AddRequestData("vnp_ReturnUrl", vnpaySettings["ReturnUrl"]);
            _vnPayLibrary.AddRequestData("vnp_TxnRef", order.OrderId.ToString());
            _vnPayLibrary.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss"));
            _vnPayLibrary.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(httpContext));

            return _vnPayLibrary.CreateRequestUrl(vnpaySettings["Url"], vnpaySettings["HashSecret"]);
        }

        public bool ValidatePaymentResponse(Dictionary<string, string> responseData)
        {
            var vnpaySettings = _configuration.GetSection("VnPaySettings");

            foreach (var item in responseData)
            {
                _vnPayLibrary.AddResponseData(item.Key, item.Value);
            }

            var secureHash = responseData["vnp_SecureHash"];
            return _vnPayLibrary.ValidateSignature(secureHash, vnpaySettings["HashSecret"]);

        }
    }
}
