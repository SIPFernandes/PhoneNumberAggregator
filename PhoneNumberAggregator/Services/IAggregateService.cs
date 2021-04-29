using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PhoneNumberAggregator.Services
{
    public interface IAggregateService
    {
        /// <summary>
        /// This method will call an external api to get the valid numbers sectors and then
        /// will count valid phones broken down per prefix and per business sector
        /// </summary>
        /// <param name="numbers">Array with phone numbers</param>
        /// <returns>
        /// Task Dictionary with prefix as key and another dictionary as value, in the second dictionary 
        /// the sectors are keys and for values, the number of times that sector appears for the 
        /// respective prefix
        /// </returns> 
        public Task<Dictionary<string, Dictionary<string, int>>> AgregatePrefixesAndSections(string[] numbers);
        /// <summary>
        /// This method will call an external api to get the valid numbers sectors
        /// </summary>
        /// <param name="numbers">Array with phone numbers</param>
        /// <returns>Task with a Task array of Sector Api responses</returns>
        public Task<Task<HttpResponseMessage>[]> RequestSectorAPI(string[] numbers);
        /// <summary>
        /// Returns the given number prefix from the prefix HashSet
        /// </summary>
        /// <param name="number">A phone number</param>
        /// <returns>The phone number prefix or null if inst valid</returns>        
        public string GetPrefixFromNumber(string number);
        /// <summary>
        /// Sets the list of prefixes in a HashSet also sets the prefixes lenght in a SortedSet
        /// </summary>        
        public void SetNewPrefixFile();
    }
}
