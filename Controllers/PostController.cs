using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
  [ApiController]
  [Authorize]
  [Route("v1/posts")]
  public class PostController : ControllerBase
  {
    private readonly BlogDataContext _context;
    public PostController(BlogDataContext context)
    {
      _context = context;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
      try
      {
        var posts = await _context.Posts
        .AsNoTracking()
        .Select(post => new ListPostsViewModel
        {
          Id = post.Id,
          Title = post.Title,
          Slug = post.Slug,
          LastUpdateDate = post.LastUpdateDate,
          Category = post.Category,
          Author = post.Author
        })
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .OrderByDescending(o => o.LastUpdateDate)
        .ToListAsync();

        return Ok(new ResultViewModel<dynamic>(new
        {
          posts.Count,
          page,
          pageSize,
          posts
        }));
      }
      catch (Exception)
      {
        return StatusCode(500, new ResultViewModel<string>("05XO04 - Falha interna no servidor"));
      }
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAsync(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
      try
      {
        var post = await _context.Posts
        .AsNoTracking()
        .Include(post => post.Category)
        .Include(post => post.Author)
        .ThenInclude(post => post.Roles)
        .FirstOrDefaultAsync(post => post.Id == id);

        if (post == null)
          return StatusCode(400, new ResultViewModel<string>("05XO05 - Post não encontrado"));

        return Ok(new ResultViewModel<Post>(post));
      }
      catch (Exception)
      {
        return StatusCode(500, new ResultViewModel<string>("05XO04 - Falha interna no servidor"));
      }
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetAsync(string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
      try
      {
        var posts = await _context.Posts
          .AsNoTracking()
          //  .Include(p => p.Category)
          .Include(post => post.Author)
          .ThenInclude(author => author.Roles)
          .Where(post => post.Category.Slug == category)
          .Select(post => new ListPostsViewModel
          {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            LastUpdateDate = post.LastUpdateDate,
            Category = post.Category,
            Author = post.Author
          })
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .OrderByDescending(order => order.LastUpdateDate)
          .ToListAsync();

        return Ok(new ResultViewModel<dynamic>(new
        {
          posts.Count,
          page,
          pageSize,
          posts
        }));
      }
      catch (Exception)
      {
        return StatusCode(500, new ResultViewModel<string>("05XO04 - Falha interna no servidor"));
      }
    }
  }
}