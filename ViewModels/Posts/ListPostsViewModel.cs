using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Models;

namespace Blog.ViewModels.Posts
{
    public class ListPostsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;
        public Category Category { get; set; } = new();
        public User Author { get; set; } = new();
    }
}