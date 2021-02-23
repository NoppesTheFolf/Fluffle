using FluentValidation;
using Noppes.Fluffle.Validation;

namespace Noppes.Fluffle.Main.Communication
{
    public class PutImageIndexModel : PutContentIndexModel
    {
        public class ImageHashesModel
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

        public ImageHashesModel Hashes { get; set; }
    }

    public class PutImageHashModelValidator : AbstractValidator<PutImageIndexModel>
    {
        public PutImageHashModelValidator()
        {
            RuleFor(o => o.Hashes.PhashRed64).Length(8);
            RuleFor(o => o.Hashes.PhashGreen64).Length(8);
            RuleFor(o => o.Hashes.PhashBlue64).Length(8);
            RuleFor(o => o.Hashes.PhashAverage64).Length(8);

            RuleFor(o => o.Hashes.PhashRed256).Length(32);
            RuleFor(o => o.Hashes.PhashGreen256).Length(32);
            RuleFor(o => o.Hashes.PhashBlue256).Length(32);
            RuleFor(o => o.Hashes.PhashAverage256).Length(32);

            RuleFor(o => o.Hashes.PhashRed1024).Length(128);
            RuleFor(o => o.Hashes.PhashGreen1024).Length(128);
            RuleFor(o => o.Hashes.PhashBlue1024).Length(128);
            RuleFor(o => o.Hashes.PhashAverage1024).Length(128);
        }
    }
}
