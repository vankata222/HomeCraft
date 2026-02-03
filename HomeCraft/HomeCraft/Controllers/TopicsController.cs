using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using HomeCraft.Data;
using HomeCraft.Data.Models; 
using HomeCraft.ViewModels.Topics;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public async Task<IActionResult> Index(string? categoryId)
        {
            var query = _context.Topics
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Votes)
                .Include(t => t.Favorites)
                .AsQueryable();

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(t => t.CategoryId == categoryId);
            }

            var topics = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TopicIndexViewModel
                {
                    Topic = t,
                    LikeCount = t.Votes.Count(v => v.IsLiked),
                    DislikeCount = t.Votes.Count(v => !v.IsLiked),
                    ShortDescription = t.Description.Length > 120 
                        ? t.Description.Substring(0, 120) + "..." 
                        : t.Description
                }).ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCategory = categoryId; 

            return View(topics);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var topic = await _context.Topics
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .Include(t => t.Votes)
                .Include(t => t.Favorites)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (topic == null) return NotFound();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            
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
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TopicCreateViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                var topic = new Topic
                {
                    Title = model.Title,
                    Description = model.Description,
                    MediaUrl = model.MediaUrl,
                    CategoryId = model.CategoryId,
                    UserId = userId, 
                    CreatedAt = DateTime.Now
                };

                _context.Topics.Add(topic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (topic.UserId != currentUserId && !User.IsInRole("Admin")) return Forbid();

            var model = new TopicEditViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                MediaUrl = topic.MediaUrl,
                CategoryId = topic.CategoryId,
                UserId = topic.UserId, 
                CreatedAt = topic.CreatedAt 
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", topic.CategoryId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, TopicEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (topic.UserId != currentUserId && !User.IsInRole("Admin")) 
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    topic.Title = model.Title;
                    topic.Description = model.Description;
                    topic.MediaUrl = model.MediaUrl;
                    topic.CategoryId = model.CategoryId;

                    _context.Update(topic);
                    await _context.SaveChangesAsync();
            
                    return RedirectToAction(nameof(Details), new { id = topic.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicExists(topic.Id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
            return View(model);
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
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string topicId)
        {
            var userId = _userManager.GetUserId(User);
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.TopicId == topicId && f.UserId == userId);

            if (favorite == null)
            {
                _context.Favorites.Add(new Favorite { TopicId = topicId, UserId = userId });
            }
            else
            {
                _context.Favorites.Remove(favorite);
            }

            await _context.SaveChangesAsync();
    
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public async Task<IActionResult> MyFavorites()
        {
            var userId = _userManager.GetUserId(User);
            var favoriteTopics = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Topic)
                .ThenInclude(t => t.User)
                .Select(f => f.Topic)
                .ToListAsync();

            return View(favoriteTopics);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MoveTopic(string topicId, string newCategoryId)
        {
            var topic = await _context.Topics.FindAsync(topicId);
            if (topic != null)
            {
                topic.CategoryId = newCategoryId;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = topicId });
        }
        private bool TopicExists(string id) => _context.Topics.Any(e => e.Id == id);
    }
}