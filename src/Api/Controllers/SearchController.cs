using Microsoft.AspNetCore.Mvc;

namespace VectorApi
{
    [ApiController]
    [Route("search")] // Controller-level route
    public class SearchController : ControllerBase
    {
        private readonly VectorDbService _dbService;

        public SearchController(VectorDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("cosine")]
        public async Task<IActionResult> SearchByCosineSimilarity(string query)
        {
            var results = await _dbService.SearchByCosineSimilarity(query);
            return Ok(results);
        }

        [HttpGet("dotproduct")]
        public async Task<IActionResult> SearchByDotProduct(string query)
        {
            var results = await _dbService.SearchByDotProduct(query);
            return Ok(results);
        }

        [HttpGet("euclidean")]
        public async Task<IActionResult> SearchByEuclideanDistance(string query)
        {
            var results = await _dbService.SearchByEuclideanDistance(query);
            return Ok(results);
        }
    }
}