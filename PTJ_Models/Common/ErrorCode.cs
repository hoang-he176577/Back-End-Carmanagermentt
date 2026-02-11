

namespace Models.Common
{
    public enum ErrorCode
    {
        UserNotFound,
        UserExisted,
        EmailConfirmFailed,
        InvalidToken,
        Unauthorized,
        RefreshTokenNotFound,
        RefreshTokenExpired,
        GeneralError,
        NotFound,
        BadRequest,
        Forbidden
    }
}

