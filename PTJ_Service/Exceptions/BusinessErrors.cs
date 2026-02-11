using Models.Common;
using System.Net;

namespace Service.Exceptions
{
    public static class BusinessErrors
    {
        // ----------- USER ERRORS -----------
        public static BusinessException UserNotFound() =>
            new BusinessException(ErrorCode.UserNotFound,
                "User not found.",
                HttpStatusCode.NotFound);

        public static BusinessException UserDuplicated() =>
            new BusinessException(ErrorCode.UserExisted,
                "User already exists.",
                HttpStatusCode.Conflict);

        public static BusinessException EmailConfirmFailed() =>
            new BusinessException(ErrorCode.EmailConfirmFailed,
                "Email confirmation failed.",
                HttpStatusCode.BadRequest);

        // ----------- TOKEN ERRORS -----------
        public static BusinessException RefreshTokenNotFound(Guid userId) =>
            new BusinessException(ErrorCode.RefreshTokenNotFound,
                $"No refresh token found for user {userId}.",
                HttpStatusCode.NotFound);

        public static BusinessException RefreshTokenExpired() =>
            new BusinessException(ErrorCode.RefreshTokenExpired,
                "Refresh token has expired.",
                HttpStatusCode.Unauthorized);

        // ----------- GENERIC ERRORS -----------
        public static BusinessException NotFound(string message) =>
            new BusinessException(ErrorCode.NotFound, message, HttpStatusCode.NotFound);

        public static BusinessException Unauthorized(string message) =>
            new BusinessException(ErrorCode.Unauthorized, message, HttpStatusCode.Unauthorized);

        public static BusinessException BadRequest(string message) =>
            new BusinessException(ErrorCode.BadRequest, message, HttpStatusCode.BadRequest);
    }
}
