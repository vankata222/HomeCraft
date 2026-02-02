using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HomeCraft.Data;
using HomeCraft.Data.Models; 
using HomeCraft.ViewModels.Topics; 

namespace HomeCraft.Controllers
{
    [Authorize]
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var topics = await _context.Topics
                .Include(t => t.Votes) 
                .Include(t => t.Comments)
                .Select(t => new TopicIndexViewModel
                {
                    Topic = t,
                    LikeCount = t.Votes.Count(v => v.IsLiked),
                    DislikeCount = t.Votes.Count(v => !v.IsLiked)
                }).ToListAsync();

            return View(topics);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var topic = await _context.Topics
                .Include(t => t.User)
                .Include(t => t.Votes) 
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (topic == null) return NotFound();

            return View(topic);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(string topicId, bool isLiked)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // Check if the user already has a unique vote for this topic
            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.TopicId == topicId && v.UserId == userId);

            if (existingVote != null) 
            {
                existingVote.IsLiked = isLiked;
                _context.Update(existingVote);
            } 
            else 
            {
                _context.Votes.Add(new Vote 
                { 
                    TopicId = topicId, 
                    UserId = userId, 
                    IsLiked = isLiked 
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = topicId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(string topicId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) 
                return RedirectToAction(nameof(Details), new { id = topicId });

            var comment = new Comment 
            {
                TopicId = topicId,
                UserId = _userManager.GetUserId(User),
                Content = content,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Details), new { id = topicId });
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TopicCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var topic = new Topic
                {
                    Title = model.Title,
                    Description = model.Description,
                    MediaUrl = model.MediaUrl,
                    UserId = _userManager.GetUserId(User), 
                    CreatedAt = DateTime.Now
                };

                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (topic.UserId != currentUserId && !User.IsInRole("Admin")) return Forbid();

            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Description,MediaUrl,UserId,CreatedAt")] Topic topic)
        {
            if (id != topic.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(topic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicExists(topic.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(topic);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var topic = await _context.Topics.Include(t => t.User).FirstOrDefaultAsync(m => m.Id == id);
            if (topic == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (topic.UserId != currentUserId && !User.IsInRole("Admin")) return Forbid();

            return View(topic);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TopicExists(string id) => _context.Topics.Any(e => e.Id == id);
    }
}