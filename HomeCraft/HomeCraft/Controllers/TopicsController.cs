using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HomeCraft.Data;
using HomeCraft.Data.Models; 
using HomeCraft.ViewModels.Topics; 

namespace HomeCraft.Controllers
{
    [Authorize] // Only registered users can access the portal
    public class TopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Topics
        // Fulfills requirement: "Listing topics with short information"
        [AllowAnonymous] // Allow guests to view the list, but not create/edit
        public async Task<IActionResult> Index()
        {
            var topics = await _context.Topics
                .Include(t => t.Reviews) // Ensure reviews are loaded
                .Select(t => new TopicIndexViewModel
                {
                    Topic = t,
                    LikeCount = t.Reviews.Count(r => r.IsLiked),
                    DislikeCount = t.Reviews.Count(r => !r.IsLiked)
                }).ToListAsync();

            return View(topics);
        }

        // GET: Topics/Details/5
        // Fulfills requirement: "Detailed info with reviews/rating"
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null) return NotFound();

            var topic = await _context.Topics
                .Include(t => t.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (topic == null) return NotFound();

            return View(topic);
        }

        // POST: Topics/Rate
        // Handles the Like/Dislike requirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(string topicId, bool isLike)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.TopicId == topicId && r.UserId == userId);

            if (existingReview == null)
            {
                _context.Reviews.Add(new Review
                {
                    TopicId = topicId,
                    UserId = userId,
                    IsLiked = isLike
                });
            }
            else
            {
                existingReview.IsLiked = isLike; // Change existing vote
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = topicId });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

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
            if (id == null) return NotFound();

            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            // SECURITY CHECK: Only allow the owner (or an Admin)
            var currentUserId = _userManager.GetUserId(User);
            if (topic.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid(); // Returns a 403 Forbidden page
            }

            return View(topic);
        }

        // POST: Topics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id,
            [Bind("Id,Title,Description,MediaUrl,UserId,CreatedAt")] Topic topic)
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

        // GET: Topics/Delete/guid
        [Authorize] // Must be logged in
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var topic = await _context.Topics
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (topic == null) return NotFound();

            // The Logic Fix: Allow if user is Owner OR Admin
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            if (topic.UserId != currentUserId && !isAdmin)
            {
                return Forbid(); // This triggers the Access Denied page if you aren't the boss or the owner
            }

            return View(topic);
        }

// POST: Topics/Delete/guid
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            // Repeat security check here for the actual deletion
            if (topic.UserId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool TopicExists(string id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }
    }
}