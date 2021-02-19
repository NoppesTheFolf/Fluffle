using System.Collections.Generic;

namespace Noppes.Fluffle.B2
{
    /// <summary>
    /// Response received when listing the names of files contained in a bucket.
    /// </summary>
    public class B2ListFileNamesResponse
    {
        public ICollection<B2File> Files { get; set; }

        public string NextFileName { get; set; }
    }
}
