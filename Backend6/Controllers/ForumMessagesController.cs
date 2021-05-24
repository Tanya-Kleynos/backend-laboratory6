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
    public class ForumMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;

        public ForumMessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
        }

        // GET: ForumMessages
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumMessages.Include(f => f.Creator).Include(f => f.ForumTopic);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ForumMessages/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages
                .Include(f => f.Creator)
                .Include(f => f.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null)
            {
                return NotFound();
            }

            return View(forumMessage);
        }

        // GET: ForumMessages/Create
        public async Task<IActionResult> Create(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await this._context.ForumTopics.SingleOrDefaultAsync(x => x.Id == id);

            if (forumTopic == null)
            {
                return NotFound();
            }

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewBag.ForumTopic = forumTopic;
            return View(new ForumMessageEditModel());
        }

        // POST: ForumMessages/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? id, ForumMessageEditModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await this._context.ForumTopics.SingleOrDefaultAsync(x => x.Id == id);

            if (forumTopic == null)
            {
                return NotFound();
            }

            var user = await this.userManager.GetUserAsync(this.HttpContext.User);
            if (ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                var forumMessage = new ForumMessage
                {
                    ForumTopicId = forumTopic.Id,
                    CreatorId = user.Id,
                    Created = now,
                    Modified = now,
                    Text = model.Text,
                    Creator = user
                };
                _context.Add(forumMessage);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new { id = forumTopic.Id });
            }
            ViewBag.ForumTopic = forumTopic;
            return View(model);
        }

        // GET: ForumMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }

            var model = new ForumMessageEditModel
            {
                Text = forumMessage.Text
            };

            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", forumMessage.CreatorId);
            ViewBag.ForumTopicId = forumMessage.ForumTopicId;
            ViewBag.ForumMessage = forumMessage;
            return View(model);
        }

        // POST: ForumMessages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumMessageEditModel model)
        {
            if (id == null)
            {
                return NotFound();
            }
            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var now = DateTime.UtcNow;
                forumMessage.Text = model.Text;
                forumMessage.Modified = now;
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new { id = forumMessage.ForumTopicId });
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", forumMessage.CreatorId);
            ViewData["ForumTopicId"] = new SelectList(_context.ForumTopics, "Id", "CreatorId", forumMessage.ForumTopicId);
            ViewBag.ForumTopicId = forumMessage.ForumTopicId;
            return View(model);
        }

        // GET: ForumMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages
                .Include(f => f.Creator)
                .Include(f => f.ForumTopic)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }
            ViewBag.ForumTopicId = forumMessage.ForumTopicId;
            return View(forumMessage);
        }

        // POST: ForumMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }
            _context.ForumMessages.Remove(forumMessage);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "ForumTopics", new { id = forumMessage.ForumTopicId });
        }
    }
}
