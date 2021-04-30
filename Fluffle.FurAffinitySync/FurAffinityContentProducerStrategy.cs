using Noppes.Fluffle.FurAffinity;
using Noppes.Fluffle.Main.Client;
using System.Threading.Tasks;

namespace Noppes.Fluffle.FurAffinitySync
{
    public class FurAffinityContentProducerStateResult
    {
        public FaResult<FaSubmission> FaResult { get; set; }
    }

    public abstract class FurAffinityContentProducerStrategy
    {
        protected readonly FluffleClient FluffleClient;
        protected readonly FurAffinityClient FaClient;
        protected readonly FurAffinitySyncClientState State;

        protected FurAffinityContentProducerStrategy(FluffleClient fluffleClient, FurAffinityClient faClient, FurAffinitySyncClientState state)
        {
            FluffleClient = fluffleClient;
            FaClient = faClient;
            State = state;
        }

        public abstract Task<FurAffinityContentProducerStateResult> NextAsync();
    }
}
