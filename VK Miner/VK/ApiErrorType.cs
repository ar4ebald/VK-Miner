namespace VK_Miner.VK
{
    public enum ApiErrorType
    {
        UnknownError = 1,
        ApplicationIsDisabled = 2,
        UnknownMethodPassed = 3,
        IncorrectSignature = 4,
        UserAuthorizationFailed = 5,
        TooManyRequestsPerSecond = 6,
        PermissionToPerformThisActionIsDenied = 7,
        InvalidRequest = 8,
        FloodControl = 9,
        InternalServerError = 10,
        InTestModeApplicationShouldBeDisabledOrUserShouldBeAuthorized = 11,
        CaptchaNeeded = 14,
        AccessDenied = 15,
        HttpAuthorizationFailed = 16,
        ValidationRequired = 17,
        ConfirmationRequired = 24,
        OneOfTheParametersSpecifiedWasMissingOrInvalid = 100,
        InvalidUserId = 113,
        InvalidTimestamp = 150,
        PermissionDenied = 600
    }
}