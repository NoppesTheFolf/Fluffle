using Noppes.Fluffle.Http;
using Noppes.Fluffle.Utils;
using SerilogTimings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noppes.Fluffle.TwitterSync.AnalyzeUsers
{
    public interface IPredictClassesData : IImageRetrieverData
    {
        public ICollection<IDictionary<ClassificationClass, double>> Classes { get; set; }
    }

    public class PredictClasses<T> : Consumer<T> where T : IPredictClassesData
    {
        private readonly IPredictionClient _predictionClient;

        public PredictClasses(IPredictionClient predictionClient)
        {
            _predictionClient = predictionClient;
        }

        public override async Task<T> ConsumeAsync(T data)
        {
            using var _ = Operation.Time("Predicting image classes for {count} images", data.Images.Count);
            data.Classes = await HttpResiliency.RunAsync(() => _predictionClient.ClassifyAsync(data.OpenStreams));

            return data;
        }
    }
}
