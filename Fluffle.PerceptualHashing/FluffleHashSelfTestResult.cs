namespace Noppes.Fluffle.PerceptualHashing
{
    public class FluffleHashSelfTestResult
    {
        public byte[] PhashRed64 { get; set; }
        public byte[] PhashGreen64 { get; set; }
        public byte[] PhashBlue64 { get; set; }
        public byte[] PhashAverage64 { get; set; }

        public byte[] PhashRed256 { get; set; }
        public byte[] PhashGreen256 { get; set; }
        public byte[] PhashBlue256 { get; set; }
        public byte[] PhashAverage256 { get; set; }

        public byte[] PhashRed1024 { get; set; }
        public byte[] PhashGreen1024 { get; set; }
        public byte[] PhashBlue1024 { get; set; }
        public byte[] PhashAverage1024 { get; set; }
    }
}
