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
        public const string WrongFormat = "Wrong_Format";


        // OTP Logic
        public const string InvalidOtp = "invalid_otp";
        public const string OtpSendFailed = "otp_send_failed";

        // Registration Flow
        public const string ProfileCreationFailed = "profile_creation_failed";

        //orders
        public const string InvalidInput = "invalid_input";
        public const string DataNotFound = "data_not_found";
        public const string InvalidAction = "invalid_action";
    }
}
