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
        public async Task<IActionResult> Get()
        {
            await _dbService.InitializeAsync();
            return Ok("Reloaded DB");
        }

        // ...other actions...
    }
}
