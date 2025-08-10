using System.Net;

namespace CareerNexus.Models.Common
{
    public class SuccessResponseModel
    {
        public int StatusCode { get; set; } = (int)HttpStatusCode.OK;
        public object Data { get; set; } = new Object[0];
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
