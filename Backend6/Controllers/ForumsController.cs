using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Backend6.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Backend6.Controllers
{
    [Authorize(Roles = ApplicationRoles.Administrators)]
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForumsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Forums
        public async Task<IActionResult> Index(Guid? forumCategoryId)
        {

            var applicationDbContext = _context.Forums.Include(f => f.ForumCategory);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        // GET: Forums/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.ForumTopics)
                .ThenInclude(c => c.Creator)
                .Include(f => f.ForumTopics)
                .ThenInclude(m => m.ForumMessages)
                .ThenInclude(c => c.Creator)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // GET: Forums/Create
        public async Task<IActionResult> Create(Guid? forumCategoryId)
        {
            if (forumCategoryId == null)
            {
                return NotFound();
            }

            var forumCategory = await this._context.ForumCategories.SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return NotFound();
            }
            //ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name");
            ViewBag.ForumCategory = forumCategory;
            return View(new ForumEditModel());
        }

        // POST: Forums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumCategoryId, ForumEditModel model)
        {
            if (forumCategoryId == null)
            {
                return NotFound();
            }

            var forumCategory = await this._context.ForumCategories.SingleOrDefaultAsync(x => x.Id == forumCategoryId);

            if (forumCategory == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var forum = new Forum
                {
                    Name = model.Name,
                    Description = model.Description,
                    ForumCategoryId = forumCategory.Id
                };

                _context.Forums.Add(forum);
                await _context.SaveChangesAsync();

                var redirect = RedirectToAction("Index");
                redirect.ControllerName = "ForumCategories";
                return redirect;

                //return RedirectToAction("Index", "ForumCategories");
            }
            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forumCategory.Id);
            return View(model);
        }

        // GET: Forums/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await this._context.Forums.Include(x => x.ForumCategory).SingleOrDefaultAsync(x => x.Id == id);

            if (forum == null)
            {
                return NotFound();
            }

            var model = new ForumEditModel
            {
                Name = forum.Name,
                Description = forum.Description,
                ForumCategoryId = forum.ForumCategoryId
            };

            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forum.ForumCategoryId);
            return View(model);
        }

        // POST: Forums/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumEditModel model)
        {
            if (id == null)
            {
                return NotFound();
            }
            var forum = await this._context.Forums.Include(x => x.ForumCategory).SingleOrDefaultAsync(x => x.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                forum.Name = model.Name;
                forum.Description = model.Description;
                forum.ForumCategoryId = model.ForumCategoryId;
                await _context.SaveChangesAsync();
                var redirect = RedirectToAction("Index");
                redirect.ControllerName = "ForumCategories";
                return redirect;
            }
            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forum.ForumCategoryId);
            return View(model);
        }

        // GET: Forums/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.ForumCategory)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // POST: Forums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forum = await _context.Forums.SingleOrDefaultAsync(m => m.Id == id);
            _context.Forums.Remove(forum);
            await _context.SaveChangesAsync();
            var redirect = RedirectToAction("Index");
            redirect.ControllerName = "ForumCategories";
            return redirect;
        }

    }
}
