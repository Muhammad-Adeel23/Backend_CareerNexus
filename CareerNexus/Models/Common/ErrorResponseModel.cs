using System.Net;

namespace CareerNexus.Models.Common
{
    public class ErrorResponseModel
    {
        public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
        public List<object> Error { get; set; } = new List<Object>();
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsException { get; set; }
    }
}
