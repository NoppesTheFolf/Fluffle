namespace Noppes.Fluffle.Api.Communication
{
    public class ApiEndpoint
    {
        private readonly string _base;

        public ApiEndpoint(bool hasApiPrefix, int version)
        {
            _base = string.Empty;

            if (hasApiPrefix)
                _base += "/api";

            _base += $"/v{version}";
        }

        public object[] Url(params object[] segments)
        {
            var finalSegments = new object[segments.Length + 1];
            finalSegments[0] = _base;

            for (var i = 0; i < segments.Length; i++)
                finalSegments[i + 1] = segments[i];

            return finalSegments;
        }
    }
}
