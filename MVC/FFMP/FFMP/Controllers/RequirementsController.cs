using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFMP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FFMP.Controllers
{
    public class RequirementsController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _cntxt;

        public RequirementsController(project_3Context context, IHttpContextAccessor cntxt)
        {
            _context = context;
            _cntxt = cntxt;
        }

        // GET: Requirements
        public async Task<IActionResult> Index()
        {
            if (!UsersController.UserAuthenticated(_cntxt))
                return RedirectToAction("Login", "Users");

            var project_3Context = _context.Requirements.Include(r => r.AuditingAuditing);
            return View(await project_3Context.ToListAsync());
        }

        // GET: Requirements/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (id == null || _context.Requirements == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements
                .Include(r => r.AuditingAuditing)
                .FirstOrDefaultAsync(m => m.ReqId == id);
            if (requirement == null)
            {
                return NotFound();
            }

            return View(requirement);
        }

        // GET: Requirements/Create
        public IActionResult Create(uint? AuditingAuditingId)
        {
            var r = new Requirement();
            r.AuditingAuditingId = AuditingAuditingId.Value;
            ViewData["AuditingAuditingId"] = AuditingAuditingId;
            return View(r);
        }

        // POST: Requirements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReqId,AuditingAuditingId,Description,Must")] Requirement requirement)
        {
            _context.Add(requirement);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "AuditingForms", new { id = requirement.AuditingAuditingId });

            if (ModelState.IsValid)
            {
                _context.Add(requirement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                       .Where(y => y.Count > 0)
                       .ToList();
            }
            ViewData["AuditingAuditingId"] = new SelectList(_context.AuditingForms, "AuditingId", "AuditingId", requirement.AuditingAuditingId);
            return RedirectToAction("Edit", "AuditingForms", new { id = requirement.AuditingAuditingId });
        }

        // GET: Requirements/Edit/5
        public async Task<IActionResult> Edit(uint? id)
        {
            if (id == null || _context.Requirements == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement == null)
            {
                return NotFound();
            }
            ViewData["AuditingAuditingId"] = new SelectList(_context.AuditingForms, "AuditingId", "AuditingId", requirement.AuditingAuditingId);
            return View(requirement);
        }

        // POST: Requirements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(uint id, [Bind("ReqId,AuditingAuditingId,Description,Must")] Requirement requirement)
        {
            if (id != requirement.ReqId)
            {
                return NotFound();
            }

            try
            {
                _context.Update(requirement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequirementExists(requirement.ReqId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Edit", "AuditingForms", new { id = requirement.AuditingAuditingId });
        }

        // GET: Requirements/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (id == null || _context.Requirements == null)
            {
                return NotFound();
            }

            var requirement = await _context.Requirements
                .Include(r => r.AuditingAuditing)
                .FirstOrDefaultAsync(m => m.ReqId == id);
            if (requirement == null)
            {
                return NotFound();
            }

            return View(requirement);
        }

        // POST: Requirements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.Requirements == null)
            {
                return Problem("Entity set 'project_3Context.Requirements'  is null.");
            }
            var requirement = await _context.Requirements.FindAsync(id);
            if (requirement != null)
            {
                _context.Requirements.Remove(requirement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "AuditingForms", new { id = requirement.AuditingAuditingId });
        }

        private bool RequirementExists(uint id)
        {
            return _context.Requirements.Any(e => e.ReqId == id);
        }
    }
}
