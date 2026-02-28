namespace Med_Map.ResponceCodes
{
    public static class ErrorCodes
    {
        // General
        public const string ValidationError = "validation_error";
        public const string InternalServerError = "internal_server_error";

        // Authentication & Account
        public const string Unauthorized = "unauthorized_access";
        public const string InvalidCredentials = "invalid_credentials";
        public const string EmailAlreadyInUse = "email_already_used";
        public const string PhoneAlreadyInUse = "phone_already_used";
        public const string EmailUnconfirmed = "email_not_verified";
        public const string UserNotFound = "user_not_found";
        public const string ActivitionFailed = "activition_failed";


        // OTP Logic
        public const string InvalidOtp = "invalid_otp";
        public const string OtpSendFailed = "otp_send_failed";

        // Registration Flow
        public const string ProfileCreationFailed = "profile_creation_failed";
    }
}
