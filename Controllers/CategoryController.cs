using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;

[ApiController]
[Route("v1/categories")]
public class CategoryController : ControllerBase
{
  private readonly BlogDataContext _context;
  private readonly IMemoryCache _cache;

  public CategoryController(BlogDataContext context, IMemoryCache cache)
  {
    _context = context;
    _cache = cache;
  }

  [HttpPost("")]
  public async Task<IActionResult> CreateAsync([FromBody] EditorCategoryViewModel model)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
    try
    {
      var category = new Category
      {
        Name = model.Name,
        Slug = model.Slug,
        Posts = []
      };

      await _context.Categories.AddAsync(category);
      await _context.SaveChangesAsync();

      return Created($"{category.Name}", new ResultViewModel<Category>(category));
    }
    catch
    {
      return StatusCode(500, new ResultViewModel<Category>("05EX01 - It was not possible create the category."));
    }
  }

  [HttpGet("")]
  public IActionResult Get()
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
    try
    {
      var categories = _cache.GetOrCreate("CategoriesCache", entry =>
      {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        return GetCategories();
      });
      if (categories == null || categories.Count == 0)
        return StatusCode(400, new ResultViewModel<List<Category>>("05EX02 - No category founded"));

      return Ok(new ResultViewModel<List<Category>>(categories));
    }
    catch
    {
      return StatusCode(500, new ResultViewModel<List<Category>>("05EX03 - It was not possible get the categories."));
    }
  }

  private List<Category> GetCategories()
  {
    return [.. _context.Categories];
  }

  [HttpGet("{id:int}")]
  public async Task<IActionResult> GetAsync([FromRoute] int id)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
    try
    {
      var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
      if (category == null) return NotFound(new ResultViewModel<Category>("05EX03.1 - Category not found."));

      return Ok(new ResultViewModel<Category>(category));
    }
    catch
    {
      return StatusCode(500, new ResultViewModel<Category>("05EX03.2 - It was not possible get the category."));
    }
  }

  [HttpPut("{id:int}")]
  public async Task<IActionResult> UpdateAsync([FromBody] EditorCategoryViewModel model, [FromRoute] int id)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
    try
    {
      var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
      if (category == null) return NotFound(new ResultViewModel<Category>("05EX04.1 - Category not found."));

      category.Name = model.Name;
      category.Slug = model.Slug;

      _context.Categories.Update(category);
      await _context.SaveChangesAsync();

      return Ok(category);
    }
    catch
    {
      return StatusCode(500, new ResultViewModel<Category>("05EX04.2 - It was not possible update the category."));
    }
  }

  [HttpDelete("{id:int}")]
  public async Task<IActionResult> DeleteAsync([FromRoute] int id)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));
    try
    {
      var model = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
      if (model == null) return NotFound(new ResultViewModel<Category>("05EX05.1 - Category not found."));

      _context.Categories.Remove(model);
      await _context.SaveChangesAsync();

      return Ok(new ResultViewModel<Category>(model));
    }
    catch
    {
      return StatusCode(500, new ResultViewModel<Category>("05EX05.2 -  was not possible delete the category."));
    }
  }
}
