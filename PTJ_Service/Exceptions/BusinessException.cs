using Models.Common;
using System.Net;

namespace Service.Exceptions
{
    public class BusinessException : Exception
    {
        public ErrorCode Code { get; }
        public HttpStatusCode StatusCode { get; }

        public BusinessException(ErrorCode code, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}
