using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFMP.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace FFMP.Controllers
{
    public class AuditingLogsController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _ctx;

        public AuditingLogsController(project_3Context context, IHttpContextAccessor ctx)
        {
            _context = context;
            _ctx = ctx;
        }

        // GET: AuditingLogs       
        public async Task<IActionResult> Index(uint? id, string? errorText)
        {
            ViewBag.ErrorText = errorText;
            if (!UsersController.UserAuthenticated(_ctx))
                return RedirectToAction("Login", "Users");

            var project_3Context = id != null ? _context.AuditingLogs.Include(a => a.Object).Include(a => a.UserLoginNavigation).Where(x => x.ObjectId == id) : _context.AuditingLogs.Include(a => a.Object).Include(a => a.UserLoginNavigation);

            var a = await project_3Context.ToListAsync();
            if (!a.Any())
            {
                var al = new AuditingLog();
                al.ObjectId = id == null ? 0 : id.Value;
                a.Add(al);
            }
            else if (id == null)
            {
                a[0].ObjectId = 0;
            }
                
            return View(a);
        }

        // GET: AuditingLogs/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (!UsersController.UserAuthenticated(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingLogs == null)
            {
                return NotFound();
            }

            var auditingLog = await _context.AuditingLogs
                .Include(a => a.Object)
                .Include(a => a.UserLoginNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditingLog == null)
            {
                return NotFound();
            }            
            return View(auditingLog);
        }

        // GET: AuditingLogs/Create
        public IActionResult Create(uint id)
        {
            if (!UsersController.UserAuthenticated(_ctx))
                return RedirectToAction("Login", "Users");
          
            var a = new AuditingLog();
            a.ObjectId = id;
            a.Result = "INCOMPLETE";
            a.Object = _context.ObjectToChecks.FirstOrDefault(x => x.Id == id);
            a.RequirementResults = new List<RequirementResult>();
            a.Created = DateTime.Now;
            var u = UsersController.GetUser(_ctx);
            a.UserLoginNavigation = _context.Users.FirstOrDefault(x => x.Name == u);

            var af = _context.AuditingForms.Where(x => x.TargetGroupId == a.Object.TargetGroupId).OrderByDescending(x => x.Created).FirstOrDefault();
            if (af == null)
            {
                ViewBag.ErrorText = "No auditing forms for this target group";
                return RedirectToAction("Index", "AuditingLogs", new { id = id, errorText = "No auditing forms for this target group" });
            }
            var reqs = _context.Requirements.Where(x => x.AuditingAuditingId == af.AuditingId);
            if (reqs == null || !reqs.Any())
            {
                ViewBag.ErrorText = "No auditing requirements for this target group"; 
                return RedirectToAction("Index", "AuditingLogs", new { id = id, errorText = "No auditing requirements for this target group" });
            }
            a.Description = af.Description;
            foreach (var r in reqs)
            {
                var rr = new RequirementResult();
                rr.Description = r.Description;
                rr.Must = r.Must;
                a.RequirementResults.Add(rr);
            }
            _context.Add(a);
            _context.SaveChanges();
            return RedirectToAction("Edit", "AuditingLogs", new { id = a.Id });
        }

        // POST: AuditingLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditingLog auditingLog)
        {
            _context.Add(auditingLog);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            if (ModelState.IsValid)
            {
                _context.Add(auditingLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ObjectId"] = new SelectList(_context.ObjectToChecks, "Id", "Id", auditingLog.ObjectId);
            ViewData["UserLogin"] = new SelectList(_context.Users, "Login", "Login", auditingLog.UserLogin);
            return View(auditingLog);
        }

        // GET: AuditingLogs/Edit/5
        public async Task<IActionResult> Edit(uint? id)
        {            
            if (!UsersController.UserAuthenticated(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingLogs == null)
            {
                return NotFound();
            }

            var auditingLog = await _context.AuditingLogs.Include(x => x.Object).Include(x => x.RequirementResults).FirstOrDefaultAsync(x => x.Id == id);
            if (auditingLog == null)
            {
                return NotFound();
            }
            ViewBag.AuditingText = "'" + auditingLog.Description + "' for " + auditingLog.Object.Name;
            ViewData["ObjectId"] = new SelectList(_context.ObjectToChecks, "Id", "Id", auditingLog.ObjectId);
            ViewData["UserLogin"] = new SelectList(_context.Users, "Login", "Login", auditingLog.UserLogin);
            return View(auditingLog);
        }

        // POST: AuditingLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(uint id, [Bind("Id,UserLogin,ObjectId,Created,Description,Result")] AuditingLog auditingLog)
        {
            if (id != auditingLog.Id)
            {
                return NotFound();
            }

            try
            {
                var rr = _context.RequirementResults.Where(x => x.AuditingLogsId == auditingLog.Id).ToList();
                auditingLog.Result = "OK";
                foreach (var r in rr)
                {
                    if (r.Result == null) { 
                        auditingLog.Result = "INCOMPLETE";
                        break;
                    }
                    if (r.Result == false && r.Must)
                        auditingLog.Result = "NOT OK";
                }

                _context.Update(auditingLog);
                // Object status is changed if latest auditing
                var la = _context.AuditingLogs.Where(x => x.ObjectId == auditingLog.ObjectId).OrderByDescending(x => x.Created).First();
                if (la.Id == auditingLog.Id)
                {
                    var o = _context.ObjectToChecks.First(x => x.Id == auditingLog.ObjectId);
                    o.State = auditingLog.Result == "NOT OK" ? false : auditingLog.Result == "OK" ? true : o.State;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AuditingLogExists(auditingLog.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Edit", "AuditingLogs", new { id = auditingLog.Id });
        }

        // GET: AuditingLogs/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (!UsersController.UserAuthenticated(_ctx))
                return RedirectToAction("Login", "Users");

            if (id == null || _context.AuditingLogs == null)
            {
                return NotFound();
            }

            var auditingLog = await _context.AuditingLogs
                .Include(a => a.Object)
                .Include(a => a.UserLoginNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditingLog == null)
            {
                return NotFound();
            }

            return View(auditingLog);
        }

        // POST: AuditingLogs/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.AuditingLogs == null)
            {
                return Problem("Entity set 'project_3Context.AuditingLogs'  is null.");
            }
            var auditingLog = await _context.AuditingLogs.Include(x => x.RequirementResults).FirstOrDefaultAsync(x => x.Id == id);
            if (auditingLog != null)
            {
                foreach (var r in auditingLog.RequirementResults)
                {
                    _context.RequirementResults.Remove(r);
                }
                _context.AuditingLogs.Remove(auditingLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "AuditingLogs", new { id = auditingLog.ObjectId });

        }

        private bool AuditingLogExists(uint id)
        {
            return _context.AuditingLogs.Any(e => e.Id == id);
        }
    }
}
