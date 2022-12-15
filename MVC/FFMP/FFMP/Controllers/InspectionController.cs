using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FFMP.Data;
using FFMP.BlobStorageServices;

namespace FFMP.Controllers
{
    public class InspectionController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _cntxt;
        private readonly IBlobStorageService _blobStorage;

        public InspectionController(project_3Context context, IHttpContextAccessor cntxt, IBlobStorageService blobStorage)
        {
            _context = context;
            _cntxt = cntxt;
            _blobStorage = blobStorage;
        }


        // GET: All Blob Files
        public async Task<IActionResult> Files()
        {
            return View(await _blobStorage.GetAllBlobFiles());
        }

        // UPLOAD VIEW
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        // UPLOAD FILE
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile files)
        {
            await _blobStorage.UploadBlobFileAsync(files);
            return RedirectToAction("Files");
        }

        // DELETE FILE
        public async Task<IActionResult> DeleteFile(string blobName)
        {
            await _blobStorage.DeleteDocumentAsync(blobName);
            return RedirectToAction("Files", "Inspection");
        }

        // DOWNLOAD FILE
        public async Task<IActionResult> DownloadFile(string blobName)
        {
            await _blobStorage.DownloadDocumentAsync(blobName);
            return RedirectToAction("Files", "Inspection");
        }


        // GET: Inspection
        public async Task<IActionResult> Index(string SortOrder, string searchByCreator)
        {
            if (!UsersController.UserAuthenticated(_cntxt))
                return RedirectToAction("Login", "Users");

            ViewData["TimestampSortParam"] = String.IsNullOrEmpty(SortOrder) ? "timestamp_sort" : "";
            ViewData["ObjectSortParam"] = SortOrder == "object_sort" ? "object_sort_desc" : "object_sort";

            var inspections = await _context.Inspections.Include(i => i.Object).Include(i => i.UserLoginNavigation).ToListAsync();

            if (!String.IsNullOrEmpty(searchByCreator))
            {
                inspections = await _context.Inspections.Include(i => i.Object)
                    .Include(i => i.UserLoginNavigation).Where(i => i.UserLoginNavigation.Name
                    .Contains(searchByCreator)).ToListAsync();
            }
            switch (SortOrder)
            {
                case "timestamp_sort":
                    inspections = inspections.OrderByDescending(i => i.Timestamp).ToList();
                    break;
                case "object_sort":
                    inspections = inspections.OrderBy(i => i.ObjectId).ToList();
                    break;
                case "object_sort_desc":
                    inspections = inspections.OrderByDescending(i => i.ObjectId).ToList();
                    break;
                default:
                    inspections = inspections.OrderBy(i => i.Timestamp).ToList();
                    break;
            }
            return View(inspections);
        }


        // GET: Inspections of Object(id)
        public async Task<IActionResult> ObjectsInspections(uint? id)
        {
            var objInspections = await _context.Inspections.Include(i => i.Object).Include(i => i.UserLoginNavigation).Where(x => x.ObjectId == id).ToListAsync();
            if (!objInspections.Any())
            {
                var o = new Inspection();
                o.ObjectId = id.Value;
                objInspections.Add(o);
            }
            return View("ObjectsInspections", objInspections);
        }


        // GET: Inspection/Details/5
        public async Task<IActionResult> Details(uint? id)
        {
            if (id == null || _context.Inspections == null)
                return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Object)
                .Include(i => i.UserLoginNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspection == null)
                return NotFound();

            return View(inspection);
        }


        // POST: Inspection/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(uint id, [Bind("Id,UserLogin,ObjectId,Timestamp,Reason,Observations,ChangeOfState,Inspectioncol")] Inspection inspection, List<IFormFile> files)
        {
            var objectInspected = _context.ObjectToChecks.Find(inspection.ObjectId);
            var insp = _context.Inspections.Find(id);

            List<string> fileNames = new List<string>();

            foreach (IFormFile file in files)
            {
                if (file.Length < 5097152)
                {
                    await _blobStorage.UploadBlobFileAsync(file);
                    fileNames.Add(file.FileName);
                }
            }
            string combinedString = string.Join(",", fileNames);

            if (id != inspection.Id)
                return NotFound();
            if (insp == null)
                return NotFound();

            insp.Id = id;
            insp.UserLogin = inspection.UserLogin;
            insp.ObjectId = inspection.ObjectId;
            insp.Timestamp = inspection.Timestamp;
            insp.Reason = inspection.Reason;
            insp.Observations = inspection.Observations;
            insp.ChangeOfState = inspection.ChangeOfState;
            insp.Inspectioncol = combinedString;

            if (insp.ChangeOfState != objectInspected!.State)
            {
                var lInst = _context.Inspections.Where(x => x.ObjectId == insp.ObjectId).OrderByDescending(x => x.Timestamp).First();
                if (lInst.Id == insp.Id)
                {
                    objectInspected.State = insp.ChangeOfState == true ? true : false;
                    _context.Update(objectInspected);
                }
            }
            _context.Update(insp);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Inspection/Delete/5
        public async Task<IActionResult> Delete(uint? id)
        {
            if (id == null || _context.Inspections == null)
                return NotFound();

            var inspection = await _context.Inspections
                .Include(i => i.Object)
                .Include(i => i.UserLoginNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspection == null)
                return NotFound();

            return View(inspection);
        }

        // POST: Inspection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(uint id)
        {
            if (_context.Inspections == null)
                return Problem("Entity set 'project_3Context.Inspections'  is null.");

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
                _context.Inspections.Remove(inspection);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InspectionExists(uint id)
        {
            return (_context.Inspections?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
