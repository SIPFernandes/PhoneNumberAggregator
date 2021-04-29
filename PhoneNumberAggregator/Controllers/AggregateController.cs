using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhoneNumberAggregator.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PhoneNumberAggregator.Controllers
{    
    [Route("[controller]")]
    [ApiController]
    public class AggregateController : ControllerBase
    {
        private readonly ILogger<AggregateController> _logger;
        private readonly IAggregateService _service;

        public AggregateController(ILogger<AggregateController> logger,
            IAggregateService service)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// A Post method that receives an array of phone numbers and returns the count of valid phone 
        /// numbers broken down per prefix and per business sector
        /// </summary>
        /// <param name="numbers">Array with phone numbers</param>
        /// <returns>
        /// Dictionary with prefix as key and another dictionary as value, in the second dictionary 
        /// the sectors are keys and value is the number of times that sector appeared for the 
        /// respective prefix
        /// </returns>        
        [HttpPost]
        public async Task<Dictionary<string, Dictionary<string, int>>> Post([FromBody] string[] numbers)
        {
            try
            {
                return await _service.AgregatePrefixesAndSections(numbers);
            }
            catch (Exception e)
            {
                _logger.LogError("Aggregate Post", e, e.Message);

                return null;
            }            
        }        
    }
}
