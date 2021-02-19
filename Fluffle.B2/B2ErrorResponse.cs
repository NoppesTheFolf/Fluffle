namespace Noppes.Fluffle.B2
{
    /// <summary>
    /// Response received from B2 API when something goes wrong.
    /// </summary>
    public class B2ErrorResponse
    {
        public int Status { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }
    }
}
