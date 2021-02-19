namespace Noppes.Fluffle.B2
{
    internal static class B2Endpoints
    {
        public static readonly string Authorize = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";

        public static readonly string GetUploadUrl = "/b2api/v2/b2_get_upload_url";

        public static readonly string ListFileNames = "/b2api/v2/b2_list_file_names";

        public static readonly string DeleteFileVersion = "/b2api/v2/b2_delete_file_version";
    }
}
