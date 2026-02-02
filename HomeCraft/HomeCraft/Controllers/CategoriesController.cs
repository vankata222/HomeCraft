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
        var userId = _userManager.GetUserId(User);

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
}