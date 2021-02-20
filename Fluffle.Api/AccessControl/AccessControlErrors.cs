namespace Noppes.Fluffle.Api.AccessControl
{
    public static class AccessControlErrors
    {
        private const string AuthenticationPrefix = "AUTHENTICATION_";

        public static V1Error InvalidApiKey() =>
            new V1Error(AuthenticationPrefix + "INVALID_API_KEY",
                "Whoa, you forged yourself a fake API key huh? " +
                "To the dungeons with you! - The API key you provided doesn't exist.");

        public static V1Error HeaderWithoutValue() =>
            new V1Error(AuthenticationPrefix + "HEADER_NO_VALUE",
                "You added the API key header and thought it was a good idea to not add the API key itself. " +
                "Interesting choice I must say.");

        public static V1Error HeaderNotSet() =>
            new V1Error(AuthenticationPrefix + "HEADER_NOT_SET",
                "Halt! Identity thyself! - You tried accessing a resource which requires you to be authenticated. " +
                "Please provide an API key.");

        private const string AuthorizationPrefix = "AUTHORIZATION_";

        public static V1Error Forbidden() =>
            new V1Error(AuthorizationPrefix + "FORBIDDEN",
                "Halt! Thou are not allowed to touch the merchandise! - " +
                "You don't have the permission(s) to access this resource.");
    }
}
