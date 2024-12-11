using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repository
{
    public class CommentRepository : ICommentRepository {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context) {
            _context = context;
        }

        public async Task<Comment> CreateAsync(Comment comment) {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> DeleteAsync(int id) {
            Comment? comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null) return null;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<List<Comment>> GetAllAsync() {
            return await _context.Comments.Include(x => x.AppUser).ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int id) {
            return await _context.Comments.Include(x => x.AppUser).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Comment?> UpdateAsync(int id, Comment comment) {
            Comment? existsComment = await _context.Comments.FindAsync(id);

            if (existsComment == null) return null;

            existsComment.Title = comment.Title;
            existsComment.Content = comment.Content;

            await _context.SaveChangesAsync();

            return existsComment;
        }
    }
}
