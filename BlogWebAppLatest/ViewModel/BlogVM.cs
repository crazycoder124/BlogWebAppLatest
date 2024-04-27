﻿using BlogWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogWebApp.ViewModel
{
    public class BlogVM
    {
        
         public int? SN { get; set; }
        public Guid Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        public string? CategoryName { get; set; }
        public DateTime PublishedDate { get; set; }
        public int? BlogCategoryId { get; set; }
        public List<BlogImageVM> BlogImages { get; set; }
        public int? TotalComments { get; set; }
        public int? TotalUpvote { get; set; }
        public int? TotalDownvote { get; set; }
        public string? UserName { get; set; }
        public string? ProfileUrl { get; set; }
    }

    public class BlogImageVM {
        public string? ImageName { get; set; }
        public string Url { get; set; }
    }


}
