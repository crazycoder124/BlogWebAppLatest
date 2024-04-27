﻿using BlogWebApp.Attributes;
using BlogWebApp.Models;
using BlogWebApp.Models.IdentityModel;
using BlogWebApp.ViewModel;
using BlogWebAppLatest.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogWebApp.Controllers
{
    [UserAuthorize]
    public class DashboardController : Controller
    { 
        private readonly ILogger<DashboardController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager; 

        public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext dbcontext,
            UserManager<User> userManager)
        {
            _logger = logger;
            _dbContext = dbcontext;
            _userManager=   userManager;
        }

        [HttpGet("dashboard")]
        public IActionResult Index(int? month = null)
        {
            var dashboardData = new DashboardData();

            // Get all-time data
            dashboardData.TotalBlogPosts = _dbContext.Blogs.Count();
            dashboardData.TotalUpvotes = _dbContext.Reactions.Count(a => a.Type == "Upvote");
            dashboardData.TotalDownvotes = _dbContext.Reactions.Count(a => a.Type == "Downvote");
            dashboardData.TotalComments = _dbContext.Comments.Count();

            dashboardData.PopularBlogPosts = _dbContext.Blogs
             .OrderByDescending(post => post.Comments.Count)
            .ThenByDescending(post => post.Reactions.Count)
            .Take(10)
            .Select(post => new PopularBlogPost
            {
                Title = post.Title,
                Body = post.Body,
                PublishedDate = post.CreationAt
                //ImageUrl = post.BlogImages.FirstOrDefault()
            })
            .ToList();

            // Filter if month is supplied
            if (month.HasValue && month >= 1 && month <= 12)
            {
                var year = DateTime.Today.Year; // Assuming current year
                var startOfMonth = new DateTime(year, month.Value, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Calculate monthly counts
                dashboardData.TotalBlogPosts = _dbContext.Blogs
                    .Count(post => post.CreationAt >= startOfMonth && post.CreationAt <= endOfMonth);
                dashboardData.TotalUpvotes = _dbContext.Reactions
                    .Count(a => a.Type == "Upvote" && a.CreationDate >= startOfMonth && a.CreationDate <= endOfMonth);
                dashboardData.TotalDownvotes = _dbContext.Reactions
                    .Count(post => post.Type == "Downvote" && post.CreationDate >= startOfMonth && post.CreationDate <= endOfMonth);
                dashboardData.TotalComments = _dbContext.Comments
                    .Count(comment => comment.CreationDate >= startOfMonth && comment.CreationDate <= endOfMonth);
            }

            return View(dashboardData);
        }


        public IActionResult Profile()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public DashboardData GetDashboardData(DateTime? month = null)
        {
            var dashboardData = new DashboardData();

            // Get all-time data
            dashboardData.TotalBlogPosts = _dbContext.Blogs.Count();
            dashboardData.TotalUpvotes = _dbContext.Reactions.Where(a=> a.Type=="Upvote").Count();
            dashboardData.TotalDownvotes = _dbContext.Reactions.Where(a => a.Type == "Downvote").Count();
            dashboardData.TotalComments = _dbContext.Comments.Count();

            // Filter if month is supplied
            if (month.HasValue)
            {
                var startOfMonth = new DateTime(month.Value.Year, month.Value.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Calculate monthly counts
                dashboardData.TotalBlogPosts = _dbContext.Blogs
                    .Count(post => post.CreationAt >= startOfMonth && post.CreationAt <= endOfMonth);
                dashboardData.TotalUpvotes = _dbContext.Reactions
                .Where(a => a.Type == "Downvote" && a.CreationDate >= startOfMonth && a.CreationDate <= endOfMonth).Count();
                dashboardData.TotalDownvotes = _dbContext.Reactions
                    .Where(post => post.Type == "Downvote" && post.CreationDate >= startOfMonth && post.CreationDate <= endOfMonth).Count();
                   
                dashboardData.TotalComments = _dbContext.Comments
                    .Count(comment => comment.CreationDate >= startOfMonth && comment.CreationDate <= endOfMonth);
            }

            return dashboardData;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardDataForBlogger(int? month = null)
        {
            var dashboardData = new DashboardData();
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                // If user is not logged in, return an error or handle it appropriately
                return BadRequest("User not found.");
            }

            var userId = user.Id;

            // Get all-time data if userId is not provided
            if (string.IsNullOrEmpty(userId))
            {
                dashboardData.TotalBlogPosts = _dbContext.Blogs.Count();
                dashboardData.TotalUpvotes = _dbContext.Reactions.Where(a => a.Type == "Upvote").Count();
                dashboardData.TotalDownvotes = _dbContext.Reactions.Where(a => a.Type == "Downvote").Count();
                dashboardData.TotalComments = _dbContext.Comments.Count();
            }
            else // Filter by userId if provided
            {
                // Filter by userId
                var userBlogsQuery = _dbContext.Blogs.Where(blog => blog.AuthorId.ToString() == userId);

                // Filter by month if provided
                if (month.HasValue && month >= 1 && month <= 12)
                {
                    var startOfMonth = new DateTime(DateTime.Now.Year, month.Value, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    userBlogsQuery = userBlogsQuery.Where(post => post.CreationAt >= startOfMonth && post.CreationAt <= endOfMonth);
                }

                // Calculate counts
                dashboardData.TotalBlogPosts = userBlogsQuery.Count();
                dashboardData.TotalUpvotes = _dbContext.Reactions.Where(a => a.Type == "Upvote" && a.UserId.ToString() == userId).Count();
                dashboardData.TotalDownvotes = _dbContext.Reactions.Where(a => a.Type == "Downvote" && a.UserId.ToString() == userId).Count();
                dashboardData.TotalComments = _dbContext.Comments.Count(comment => comment.CommentedBy.ToString() == userId);
            }

            return Ok(new { dashboardData = dashboardData });
        }

        [HttpGet]
        public IActionResult GetTopBloggerUsers(DateTime? month = null)
        {
            var query = _dbContext.Users; // Directly accessing the Users table

            if (month.HasValue)
            {
                var startOfMonth = new DateTime(month.Value.Year, month.Value.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                query = (Microsoft.EntityFrameworkCore.DbSet<User>)query.OrderByDescending(u =>
               _dbContext.Blogs.Count(b => b.AuthorId == Guid.Parse(u.Id) && b.CreationAt >= startOfMonth && b.CreationAt <= endOfMonth) +
               _dbContext.Comments.Count(c => c.CommentedBy == Guid.Parse(u.Id) && c.CreationDate >= startOfMonth && c.CreationDate <= endOfMonth));
            }
            else
            {
                query = (Microsoft.EntityFrameworkCore.DbSet<User>)query.OrderByDescending(u =>
                    _dbContext.Blogs.Count(b => b.AuthorId.ToString() == u.Id) +
                    _dbContext.Comments.Count(c => c.CommentedBy == Guid.Parse(u.Id)));
            }

            var topUsers = query.Take(10).ToList();

            return Ok(topUsers);

        }

        [HttpGet]
        public IActionResult GetTopBlogs(DateTime? month = null)
        {
            var query = _dbContext.Blogs.AsQueryable();

            if (month.HasValue)
            {
                var startOfMonth = new DateTime(month.Value.Year, month.Value.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                query = query.Where(b => b.CreationAt >= startOfMonth && b.CreationAt <= endOfMonth);
            }

            var topBlogs = query.OrderByDescending(b =>
                    (_dbContext.Reactions.Count(r => r.EntityId == b.Id && r.Type == "Upvote") -
                    _dbContext.Reactions.Count(r => r.EntityId == b.Id && r.Type == "Downvote")) +
                    _dbContext.Comments.Count(c => c.BlogId == b.Id))
                .Take(10)
                .ToList();

            return Ok(topBlogs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please Fill Required Field";
                return View("profile");
            }

            var user = await _userManager.FindByIdAsync(viewModel.Id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Please Fill Required Field";
                return RedirectToAction("profile");
                //return NotFound();
            }

            user.DisplayName = viewModel.UserName;
            user.UserName = viewModel.Email;
            user.Email = viewModel.Email;
            user.PhoneNumber = viewModel.PhoneNumber;
            user.Address = viewModel.Address;
            user.Position = viewModel.Position;
            user.Bio = viewModel.Bio;
            user.Country = viewModel.Country;
            user.ProfileUrl = viewModel.ProfileUrl;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User Details updated Successfully.";
                return View("profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                return View("profile");
            }

            return null;
        }

    }
}

