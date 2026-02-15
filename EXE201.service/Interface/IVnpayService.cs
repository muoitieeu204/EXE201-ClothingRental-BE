using EXE201.Service.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.Service.Interface
{
    public interface IVnpayService
    {
        string CreatePaymentURL(OrderInfoDTO order, HttpContext httpContext);
        bool ValidatePaymentResponse(Dictionary<string, string> responseData);
    }
}
