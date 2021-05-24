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
using Backend6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumTopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumTopicsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumTopics
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumTopics.Include(f => f.Creator).Include(f => f.Forum);
            return View(await applicationDbContext.ToListAsync());
        }

        [AllowAnonymous]
        // GET: ForumTopics/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .Include(f => f.ForumMessages)
                .ThenInclude(m => m.Creator)
                .Include(f => f.ForumMessages)
                .ThenInclude(m => m.Attachments)
                .SingleOrDefaultAsync(f => f.Id == id);
            if (forumTopic == null)
            {
                return NotFound();
            }

            return View(forumTopic);
        }

        // GET: ForumTopics/Create
        public async Task<IActionResult> Create(Guid? forumId)
        {
            if (forumId == null)
            {
                return NotFound();
            }

            var forum = await this._context.Forums.SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return NotFound();
            }

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            this.ViewBag.Forum = forum;
            return View(new ForumTopicEditModel());
        }

        // POST: ForumTopics/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? forumId, ForumTopicEditModel model)
        {
            if (forumId == null)
            {
                return NotFound();
            }

            var forum = await this._context.Forums.SingleOrDefaultAsync(x => x.Id == forumId);

            if (forum == null)
            {
                return NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);
            if (ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                var forumTopic = new ForumTopic
                {
                    Created = now,
                    Name = model.Name,
                    CreatorId = user.Id,
                    ForumId = forum.Id,
                    Creator = user
                };

                _context.Add(forumTopic);
                await _context.SaveChangesAsync();
                return this.RedirectToAction("Details", "Forums", new { id = forum.Id });
            }
            this.ViewBag.Forum = forum;
            return View(model);
        }

        // GET: ForumTopics/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null || !this.userPermissions.CanEditForumTopic(forumTopic))
            {
                return NotFound();
            }

            var model = new ForumTopicEditModel
            {
                Name = forumTopic.Name
            };

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", forumTopic.CreatorId);
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name", forumTopic.ForumId);
            ViewBag.ForumTopicId = forumTopic.Id;
            return View(model);
        }

        // POST: ForumTopics/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumTopicEditModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null || !this.userPermissions.CanEditForumTopic(forumTopic))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                forumTopic.Name = model.Name;
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = forumTopic.Id });
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", forumTopic.CreatorId);
            ViewData["ForumId"] = new SelectList(_context.Forums, "Id", "Name", forumTopic.ForumId);
            ViewBag.ForumId = forumTopic.ForumId;
            return View(model);
        }

        // GET: ForumTopics/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null || !this.userPermissions.CanEditForumTopic(forumTopic))
            {
                return NotFound();
            }
            ViewBag.ForumTopicId = forumTopic.Id;
            return View(forumTopic);
        }

        // POST: ForumTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            var forumTopic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null || !this.userPermissions.CanEditForumTopic(forumTopic))
            {
                return NotFound();
            }
            _context.ForumTopics.Remove(forumTopic);
            await _context.SaveChangesAsync();
            return this.RedirectToAction("Details", "Forums", new { id = forumTopic.ForumId });
        }

    }
}
