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

            if (string.IsNullOrEmpty(username)) return Unauthorized("No username found in the token");

            if (username == null) return NotFound("User not found");

            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser == null) return Unauthorized("User not found");

            var userPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);
            return Ok(userPortfolio);
        }
    }
}
