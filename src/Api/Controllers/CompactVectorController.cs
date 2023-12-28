using Microsoft.AspNetCore.Mvc;

namespace VectorApi
{
    [ApiController]
    [Route("[controller]")]
    public class CompactVectorController : ControllerBase
    {
        private readonly VectorDbService _dbService;
        public CompactVectorController(VectorDbService dbService)
        {
            _dbService = dbService;
        }
        // Implement your API logic here

        [HttpGet("reload")]
        public async Task<IActionResult> Get(string jsonFileName)
        {
            await _dbService.InitializeAsync(jsonFileName);
            return Ok("Reloaded DB");
        }
        // route that accepts a csv file name and json file to be created

        [HttpGet("load")]
        public async Task<IActionResult> Get(string csvFileName, string jsonFileName)
        {
            await _dbService.CreateDatabaseAsync(csvFileName,jsonFileName);            
            return Ok($"Read CSV {csvFileName} and created DB {jsonFileName}");
        }

        // ...other actions...
    }
}
