using HomeCraft.Data;
using HomeCraft.Data.Models;
using HomeCraft.ViewModels.Topics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeCraft.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? categoryId)
    {
        var query = _context.Topics
            .Include(t => t.Category)
            .Include(t => t.Votes)
            .Include(t => t.Favorites)
            .AsQueryable();

        if (!string.IsNullOrEmpty(categoryId))
        {
            query = query.Where(t => t.CategoryId == categoryId);
        }

        var topics = await query.Select(t => new TopicIndexViewModel
        {
            Topic = t,
            LikeCount = t.Votes.Count(v => v.IsLiked),
            DislikeCount = t.Votes.Count(v => !v.IsLiked),
            ShortDescription = t.Description.Length > 100 
                ? t.Description.Substring(0, 100) + "..." 
                : t.Description
        }).ToListAsync();

        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.SelectedCategory = categoryId;

        return View(topics);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Manage()
    {
        var categories = await _context.Categories
            .Include(c => c.Topics)
            .ToListAsync();
        return View(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var category = await _context.Categories
            .Include(c => c.Topics)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.Topics.Any())
        {
            TempData["Error"] = "Move topics to another category before deleting this one.";
            return RedirectToAction(nameof(Manage));
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Manage));
    }
    
    // GET: Categories/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        return View(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Icon")] Category category)
    {
        if (id != category.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == category.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Manage));
        }
        return View(category);
    }
}