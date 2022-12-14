using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FFMP.Data;

namespace FFMP.Controllers
{
    public class RequirementResultsController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _cntxt;

        public RequirementResultsController(project_3Context context, IHttpContextAccessor cntxt)
        {
            _context = context;
            _cntxt = cntxt;
        }

        // GET: RequirementResults
        public async Task<IActionResult> Index()
        {
            if (!UsersController.UserAuthenticated(_cntxt))
                return RedirectToAction("Login", "Users");

            var project_3Context = _context.RequirementResults.Include(r => r.AuditingLogs);
            return View(await project_3Context.ToListAsync());
        }

        // GET: RequirementResults/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (id == null || _context.RequirementResults == null)
            {
                return NotFound();
            }

            var requirementResult = await _context.RequirementResults
                .Include(r => r.AuditingLogs)
                .FirstOrDefaultAsync(m => m.RequirementId == id);
            if (requirementResult == null)
            {
                return NotFound();
            }

            return View(requirementResult);
        }

        // GET: RequirementResults/Create
        public IActionResult Create()
        {
            ViewData["AuditingLogsId"] = new SelectList(_context.AuditingLogs, "Id", "Id");
            return View();
        }

        // POST: RequirementResults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RequirementId,AuditingLogsId,Description,Must,Result")] RequirementResult requirementResult)
        {
            _context.Add(requirementResult);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                _context.Add(requirementResult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuditingLogsId"] = new SelectList(_context.AuditingLogs, "Id", "Id", requirementResult.AuditingLogsId);
            return View(requirementResult);
        }

        // GET: RequirementResults/Edit/5
        public async Task<IActionResult> Edit(uint? id)
        {
            if (id == null || _context.RequirementResults == null)
            {
                return NotFound();
            }

            var requirementResult = await _context.RequirementResults.FindAsync(id);
            if (requirementResult == null)
            {
                return NotFound();
            }
            ViewData["AuditingLogsId"] = new SelectList(_context.AuditingLogs, "Id", "Id", requirementResult.AuditingLogsId);
            return View(requirementResult);
        }

        // POST: RequirementResults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostEdit(uint id, [Bind("RequirementId,AuditingLogsId,Description,Must,Result")] RequirementResult requirementResult)
        {
            if (id != requirementResult.RequirementId)
            {
                return NotFound();
            }

            try
            {

                if (requirementResult.Result == null)
                    requirementResult.Result = false;
                _context.Update(requirementResult);

                var rr = _context.RequirementResults.Where(x => x.AuditingLogsId == requirementResult.AuditingLogsId).ToList();
                bool? result = true;
                foreach (var r in rr)
                {
                    if (r.Result == null) { 
                        result = null;
                        break;
                    }
                    if (r.Result == false && r.Must)
                    {
                        result = false;
                    }
                }
                var a = _context.AuditingLogs.First(x => x.Id == requirementResult.AuditingLogsId);                               
                a.Result = result == false ? "NOT OK" : result == true ? "OK" : "INCOMPLETE";

                // Object status is changed if latest auditing
                var la = _context.AuditingLogs.Where(x => x.ObjectId == a.ObjectId).OrderByDescending(x => x.Created).First();
                if (la.Id == a.Id) { 
                    var o = _context.ObjectToChecks.First(x => x.Id == a.ObjectId);
                    o.State = a.Result == "NOT OK" ? false : a.Result == "OK" ? true : o.State;
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RequirementResultExists(requirementResult.RequirementId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Edit", "AuditingLogs", new { id = requirementResult.AuditingLogsId });
        }

        // GET: RequirementResults/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (id == null || _context.RequirementResults == null)
            {
                return NotFound();
            }

            var requirementResult = await _context.RequirementResults
                .Include(r => r.AuditingLogs)
                .FirstOrDefaultAsync(m => m.RequirementId == id);
            if (requirementResult == null)
            {
                return NotFound();
            }

            return View(requirementResult);
        }

        // POST: RequirementResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.RequirementResults == null)
            {
                return Problem("Entity set 'project_3Context.RequirementResults'  is null.");
            }
            var requirementResult = await _context.RequirementResults.FindAsync(id);
            if (requirementResult != null)
            {
                _context.RequirementResults.Remove(requirementResult);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequirementResultExists(uint id)
        {
            return _context.RequirementResults.Any(e => e.RequirementId == id);
        }
    }
}
