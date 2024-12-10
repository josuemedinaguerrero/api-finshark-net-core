using System.Security.Claims;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly ILogger<PortfolioController> _logger;

        public PortfolioController(
            UserManager<AppUser> userManager,
            IStockRepository stockRepository,
            IPortfolioRepository portfolioRepository,
            ILogger<PortfolioController> logger
        ) {
            _userManager = userManager;
            _stockRepository = stockRepository;
            _portfolioRepository = portfolioRepository;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio() {
            var username = User.FindFirst(ClaimTypes.GivenName)?.Value;
            _logger.LogInformation("Username JWT: {username}", username);
            if (username == null) return NotFound("No username found in the token");

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found");

            var userPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);
            return Ok(userPortfolio);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol) {
            var username = User.FindFirst(ClaimTypes.GivenName)?.Value;
            if (username == null) return NotFound("No username found in the token");

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found");

            var stock = await _stockRepository.GetBySymbolAsync(symbol);
            if (stock == null) return BadRequest("Stock not found");

            var userPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);

            if (userPortfolio.Any(e => e.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Cannot add same stock to portfolio");

            var portfolioModel = new Portfolio {
                StockId = stock.Id,
                AppUserId = appUser.Id
            };

            await _portfolioRepository.CreateAsync(portfolioModel);

            if (portfolioModel == null) return StatusCode(500, "Could not create");

            return Created();
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol) {
            var username = User.FindFirst(ClaimTypes.GivenName)?.Value;
            if (username == null) return NotFound("No username found in the token");

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found");

            var userPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);

            var filteredStock = userPortfolio.Where(s => s.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));

            if (filteredStock.Count() == 1) {
                await _portfolioRepository.DeletePortfolio(appUser, symbol);
            } else {
                return BadRequest("Stock not in your portfolio");
            }

            return Ok();
        }
    }
}
