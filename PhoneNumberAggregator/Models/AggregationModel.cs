using System.Collections.Generic;

namespace PhoneNumberAggregator.Models
{
    public class AggregationModel
    {
        public string Prefix { get; set; }
        public Dictionary<string, int> SectorCount { get; set; }
    }
}
