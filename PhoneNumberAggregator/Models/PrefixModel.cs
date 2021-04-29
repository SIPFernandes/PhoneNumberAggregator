using System.Collections.Generic;

namespace PhoneNumberAggregator.Models
{
    public class PrefixModel
    {
        public SortedSet<int> PrefixLenght { get; set; } = new SortedSet<int>();
        public HashSet<string> PrefixSet { get; set; } = new HashSet<string>();
    }
}
