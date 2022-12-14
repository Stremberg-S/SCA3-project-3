using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFMP.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FFMP.Controllers
{
    public class AuditingFormsController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _ctx;
        public AuditingFormsController(project_3Context context, IHttpContextAccessor ctx)
        {
            _context = context;
            _ctx = ctx;
        }

        // GET: AuditingForms
        public async Task<IActionResult> Index()
        {
            if (!UsersController.UserAuthenticated(_ctx))
                ///!UsersController.UserAuthenticatedAdmin(_ctx)
                return RedirectToAction("Login", "Users");

            var project_3Context = _context.AuditingForms.Include(a => a.TargetGroup).Include(a => a.UserLoginNavigation);
            return View(await project_3Context.ToListAsync());
        }

        // GET: AuditingForms/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (!UsersController.UserAuthenticatedAdmin(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingForms == null)
            {
                return NotFound();
            }

            var auditingForm = await _context.AuditingForms
                .Include(a => a.TargetGroup)
                .Include(a => a.UserLoginNavigation)
                .Include(a => a.Requirements)
                .FirstOrDefaultAsync(m => m.AuditingId == id);
            if (auditingForm == null)
            {
                return NotFound();
            }

            return View(auditingForm);
        }

        // GET: AuditingForms/Create
        public IActionResult Create()
        {
            if (!UsersController.UserAuthenticatedAdmin(_ctx))
                return RedirectToAction("Login", "Users");

            ViewData["TargetGroupId"] = new SelectList(_context.TargetGroups, "Id", "Description");
            ViewData["UserLogin"] = new SelectList(_context.Users, "Login", "Login");
            return View();
        }

        // POST: AuditingForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AuditingId,UserLogin,TargetGroupId,Created,Description")] AuditingForm auditingForm)
        {
            auditingForm.Created = DateTime.Now;

            _context.Add(auditingForm);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "AuditingForms", new { id = auditingForm.AuditingId });
        }
        // GET: AuditingForms/Edit/5
        public async Task<IActionResult> Edit(uint? id)
        {
            if (!UsersController.UserAuthenticatedAdmin(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingForms == null)
            {
                return NotFound();
            }

            var auditingForm = await _context.AuditingForms.Include(a => a.Requirements).FirstOrDefaultAsync(x => x.AuditingId == id);
            if (auditingForm == null)
            {
                return NotFound();
            }
            ViewData["TargetGroupId"] = new SelectList(_context.TargetGroups, "Id", "Description", auditingForm.TargetGroupId);
            ViewData["UserLogin"] = new SelectList(_context.Users, "Login", "Login", auditingForm.UserLogin);
            return View(auditingForm);
        }

        // POST: AuditingForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(uint id, string actionType, [Bind("AuditingId,UserLogin,TargetGroupId,Created,Description")] AuditingForm auditingForm)
        {
            if (id != auditingForm.AuditingId)
            {
                return NotFound();
            }


            try
            {
                if (actionType != "Save")
                {
                    var req = _context.Requirements.Where(x => x.AuditingAuditingId == auditingForm.AuditingId).ToList();
                    auditingForm.AuditingId = 0;
                    auditingForm.Created = DateTime.Now;
                    auditingForm.Requirements = null;
                    _context.Add(auditingForm);
                    await _context.SaveChangesAsync();

                    foreach (var r in req) {
                        r.AuditingAuditingId = auditingForm.AuditingId;
                        r.ReqId = 0;
                        _context.Requirements.Add(r);
                    }
                    await _context.SaveChangesAsync();
                }
                else { 
                _context.Update(auditingForm);
                await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuditingFormExists(auditingForm.AuditingId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Edit", "AuditingForms", new { id = auditingForm.AuditingId });
        }

        // GET: AuditingForms/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (!UsersController.UserAuthenticatedAdmin(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingForms == null)
            {
                return NotFound();
            }

            var auditingForm = await _context.AuditingForms
                .Include(a => a.TargetGroup)
                .Include(a => a.UserLoginNavigation)
                .FirstOrDefaultAsync(m => m.AuditingId == id);
            if (auditingForm == null)
            {
                return NotFound();
            }

            return View(auditingForm);
        }

        // POST: AuditingForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.AuditingForms == null)
            {
                return Problem("Entity set 'project_3Context.AuditingForms'  is null.");
            }            

            var auditingForm = await _context.AuditingForms.Include(a => a.Requirements).FirstOrDefaultAsync(x => x.AuditingId == id);
            if (auditingForm != null)
            {
                foreach (var r in auditingForm.Requirements)
                {
                    _context.Requirements.Remove(r);
                }
                _context.AuditingForms.Remove(auditingForm);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditingFormExists(uint id)
        {
            return _context.AuditingForms.Any(e => e.AuditingId == id);
        }
    }
}
