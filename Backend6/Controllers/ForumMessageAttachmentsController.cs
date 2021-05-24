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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;
using System.IO;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumMessageAttachmentsController : Controller
    {
        private static readonly HashSet<String> AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsService userPermissions;
        private readonly IHostingEnvironment hostingEnvironment;

        public ForumMessageAttachmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserPermissionsService userPermissions, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            this.userManager = userManager;
            this.userPermissions = userPermissions;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: ForumMessageAttachments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumMessageAttachments.Include(f => f.ForumMessage);
            return View(await applicationDbContext.ToListAsync());
        }



        // GET: ForumMessageAttachments/Create
        public async Task<IActionResult> Create(Guid? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forumMessage = await this._context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }

            ViewBag.ForumTopicId = forumMessage.ForumTopicId;
            return View(new ForumMessageAttachmentEditModel());
        }

        // POST: ForumMessageAttachments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid? id, ForumMessageAttachmentEditModel model)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var forumMessage = await this._context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null || !this.userPermissions.CanEditForumMessage(forumMessage))
            {
                return NotFound();
            }

            var fileName = Path.GetFileName(ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);
            if (!ForumMessageAttachmentsController.AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
            }

            if (ModelState.IsValid)
            {
                var forumMessageAttachment = new ForumMessageAttachment
                {
                    ForumMessageId = forumMessage.Id,
                    Created = DateTime.UtcNow,
                    FileName = model.FileName
                };

                var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", forumMessageAttachment.Id.ToString("N") + fileExt);
                forumMessageAttachment.FilePath = $"/attachments/{forumMessageAttachment.Id:N}{fileExt}";
                using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                _context.Add(forumMessageAttachment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "ForumTopics", new { id = forumMessage.ForumTopicId });
            }
            ViewBag.ForumTopicId = forumMessage.ForumTopicId;
            return View(model);
        }


        // GET: ForumMessageAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .Include(f => f.ForumMessage)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null || !this.userPermissions.CanEditForumMessage(forumMessageAttachment.ForumMessage))
            {
                return NotFound();
            }

            ViewBag.ForumTopicId = forumMessageAttachment.ForumMessage.ForumTopicId;
            return View(forumMessageAttachment);
        }

        // POST: ForumMessageAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments
                .Include(f => f.ForumMessage)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null || !this.userPermissions.CanEditForumMessage(forumMessageAttachment.ForumMessage))
            {
                return NotFound();
            }

            var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments", forumMessageAttachment.Id.ToString("N") + Path.GetExtension(forumMessageAttachment.FilePath));
            System.IO.File.Delete(attachmentPath);
            _context.ForumMessageAttachments.Remove(forumMessageAttachment);
            await _context.SaveChangesAsync();
            ViewBag.ForumTopicId = forumMessageAttachment.ForumMessage.ForumTopicId;
            return RedirectToAction("Details", "ForumTopics", new { id = forumMessageAttachment.ForumMessage.ForumTopicId });
        }

    }
}
