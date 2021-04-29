using Microsoft.Extensions.Configuration;
using PhoneNumberAggregator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PhoneNumberAggregator.Services
{
    public class AggregateService : IAggregateService
    {        
        private const string PrefixFilePath = "PrefixFilePath";
        private const string SectorAPIURL = "SectorAPIURL";
        private readonly IConfiguration _configuration;
        private PrefixModel _prefixModel;

        public AggregateService(IConfiguration configuration)
        {
            _configuration = configuration;

            SetNewPrefixFile();
        }

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
        public async Task<Dictionary<string, Dictionary<string, int>>> AgregatePrefixesAndSections(string[] numbers)
        {                        
            var tasks = await RequestSectorAPI(numbers);

            var result = new Dictionary<string, Dictionary<string, int>>();

            foreach (var task in tasks)
            {
                var response = task.Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var obj = await response.Content.ReadFromJsonAsync<SectorAPIModel>();

                    var prefix = GetPrefixFromNumber(obj.Number);

                    if (prefix != null)
                    {
                        AgregatePrefix(result, obj.Sector, prefix);
                    }                    
                }
            }

            return result;
        }       

        /// <summary>
        /// This method will call an external api to get the valid numbers sectors
        /// </summary>
        /// <param name="numbers">Array with phone numbers</param>
        /// <returns>Task with a Task array of Sector Api responses</returns>
        public async Task<Task<HttpResponseMessage>[]> RequestSectorAPI(string[] numbers)
        {
            var client = new HttpClient();

            var url = _configuration.GetSection(SectorAPIURL).Value;

            var tasks = new Task<HttpResponseMessage>[numbers.Length];

            for (int i = 0; i < numbers.Length; i++)
            {
                var fullUrl = Uri.EscapeUriString(url + numbers[i]);

                tasks[i] = client.GetAsync(fullUrl);
            };

            await Task.WhenAll(tasks).ConfigureAwait(false);

            return tasks;
        }

        /// <summary>
        /// Returns the given number prefix from the prefix HashSet
        /// </summary>
        /// <param name="number">A phone number</param>
        /// <returns>The phone number prefix or null if inst valid</returns>        
        public string GetPrefixFromNumber(string number)
        {
            string prefix = null;

            if (number[0] == '+')
            {
                number = number.Remove(0, 1);
            }

            foreach (var prefixLenght in _prefixModel.PrefixLenght.Reverse())
            {
                if (number.Length > prefixLenght)
                {
                    var search = number.Substring(0, prefixLenght);

                    if (_prefixModel.PrefixSet.Contains(search))
                    {
                        prefix = search;

                        break;
                    }
                }
            }

            return prefix;
        }

        /// <summary>
        /// Sets the list of prefixes in a HashSet also sets the prefixes lenght in a SortedSet
        /// </summary>        
        public void SetNewPrefixFile()
        {
            var prefixModel = new PrefixModel();

            var path = _configuration.GetSection(PrefixFilePath).Value;

            using var file = new StreamReader(path);

            while (!file.EndOfStream)
            {
                var value = file.ReadLine();

                prefixModel.PrefixLenght.Add(value.Length);

                prefixModel.PrefixSet.Add(value);
            }

            _prefixModel = prefixModel;
        }        

        /// <summary>
        /// Adds new prefix to the dictionary and the related sector if doesnt exists 
        /// or increments the respective existing sector value  
        /// </summary>
        /// <param name="result">
        /// The dictionary with the count of valid phones broken down per prefix and per business sector
        /// </param>
        /// <param name="obj">A response from Sector API with phone and sector</param>
        /// <param name="prefix">The phone prefix</param>        
        private static void AgregatePrefix(Dictionary<string, Dictionary<string, int>> result, string sector, string prefix)
        {
            if (result.ContainsKey(prefix))
            {
                var dict = result[prefix];

                if (dict.ContainsKey(sector))
                {
                    dict[sector]++;
                }
                else
                {
                    dict.Add(sector, 1);
                }
            }
            else
            {
                result.Add(prefix, new Dictionary<string, int>());

                var dict = result[prefix];

                dict.Add(sector, 1);
            }
        }        
    }
}
