using System.Security.Claims;
using api.Dtos.Comment;
using api.Interfaces;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/comment")]
    [ApiController]
    public class CommentController : ControllerBase {
        private readonly ICommentRepository _commentRepository;
        private readonly IStockRepository _stockRepository;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(
            ICommentRepository commentRepository,
            IStockRepository stockRepository,
            UserManager<AppUser> userManager
        ) {
            _commentRepository = commentRepository;
            _stockRepository = stockRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var comments = await _commentRepository.GetAllAsync();
            var commentDto = comments.Select(s => s.ToCommentDto());
            return Ok(commentDto);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id) {
            Comment? comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null) return NotFound();

            return Ok(comment.ToCommentDto());
        }

        [HttpPost]
        [Authorize]
        [Route("{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, CreateCommentDto commentDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _stockRepository.StockExists(stockId)) return BadRequest("Stock does not exist");

            var username = User.FindFirst(ClaimTypes.GivenName)?.Value;
            if (username == null) return NotFound("No username found in the token");

            var appUser = await _userManager.FindByNameAsync(username);
            if (appUser == null) return Unauthorized("User not found");

            Comment commentModel = commentDto.ToCommentFromCreate(stockId);
            commentModel.AppUserId = appUser.Id;
            await _commentRepository.CreateAsync(commentModel);
            return CreatedAtAction(nameof(GetById), new { id = commentModel.Id }, commentModel.ToCommentDto());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, UpdateCommentRequestDto commentRequestDto) {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Comment? comment = await _commentRepository.UpdateAsync(id, commentRequestDto.ToCommentFromUpdate());

            if (comment == null) return NotFound("Comment not found");

            return Ok(comment.ToCommentDto());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {
            Comment? comment = await _commentRepository.DeleteAsync(id);

            if (comment == null) return NotFound("Comment does not exist");

            return Ok(comment);
        }
    }
}
