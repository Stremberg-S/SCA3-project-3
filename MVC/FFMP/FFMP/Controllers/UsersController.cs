using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FFMP.Data;
using System.Security.Cryptography;
using System.Text;
using NuGet.Protocol.Plugins;



//using AspNetCore;

namespace FFMP.Controllers
{
    public class UsersController : Controller
    {
        private readonly project_3Context _context;
        private readonly IHttpContextAccessor _cntxt;


        public UsersController(project_3Context context, IHttpContextAccessor cntxt)
        {
            _context = context;
            _cntxt = cntxt;
        }

        public IActionResult Login()
        {

            if (UserAuthenticated(_cntxt))
                return View("AdminLanding/Index");

            return View();
        }
        

        // LOGIN CHECK
        [HttpPost]
        public async Task<IActionResult> Login(string login, string password)
        {
            var hashPassword = HashSh1($"{password}");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login && u.Password == hashPassword!);
            if (user == null)
            {
                ViewBag.Message = "Wrong username or password";
                return View("Login");

            }
            
            
            
            if (user.Admin == true && user.Active == true)
            {
                _cntxt.HttpContext?.Session.SetString("username", user.Name);
                _cntxt.HttpContext?.Session.SetString("userlogin", user.Login);
                _cntxt.HttpContext?.Session.SetString("onadmin", "Admin");


                user.LastLogin = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return View("AdminLanding/Index");
            }
            else if (user.Admin == false && user.Active == true)
            {
                _cntxt.HttpContext?.Session.SetString("username", user.Name);
                _cntxt.HttpContext?.Session.SetString("userlogin", user.Login);
                _cntxt.HttpContext?.Session.SetString("onuser", "User");

                user.LastLogin = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { id = login });
            }
            else
                ViewBag.Message = "Deactivated login information";
            return View("Login");

        }

        public async Task<IActionResult> Logout()
        {
            _cntxt.HttpContext.Session.Clear();
            return View("Login");
        }

        // GET: Users
        public async Task<IActionResult> IndexUsers(string SortOrder)
        {
            if (!UsersController.UserAuthenticated(_cntxt))
                return RedirectToAction("Login", "Users");

            var users = await _context.Users.Where(x => x.Admin == false).ToListAsync();
            
            ViewData["NameSortParam"] = String.IsNullOrEmpty(SortOrder) ? "name_sort" : "";
            ViewData["CreatedSortParam"] = SortOrder == "" ? "created_sort" : "created_sort";

            switch (SortOrder)
            {
                case "name_sort":
                    users = await _context.Users.Where(x => x.Admin == false).OrderBy(x => x.Name).ToListAsync();
                    break;
                case "created_sort":
                    users = await _context.Users.Where(x => x.Admin == false).OrderBy(x => x.Created).ToListAsync();
                    break;

            }

            return View(users);
        }

        // GET: user information for user index

        public async Task<IActionResult> Index(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Login == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Login == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            User user = new User();
            return PartialView("_CreatePartialView", user);
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind($"Name,Login,Password,Created,Admin,Active")] User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = HashSh1(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexUsers));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditAdminPartialView", user);
        }

        public async Task<IActionResult> EditForUser(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView("_EditUserPartialView", user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Name,Login,Password,Created,Admin,Active")] User user)
        {
            if (id != user.Login)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    var otc = await _context.Users.FindAsync(id);
                    if (null == otc)
                    {
                        return NotFound();
                    }
                    otc.Name = user.Name;
                    otc.Login = user.Login;
                    if (otc.Password != user.Password)
                        otc.Password = HashSh1(user.Password);
                    else otc.Password = user.Password;
                    otc.Created = user.Created;
                    otc.Admin = user.Admin;
                    otc.Active = user.Active;

                    _context.Update(otc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Login))
                    {
                        return NotFound();
                    }
                    
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexUsers));
            }
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForUser(string id, [Bind("Name,Login,Password,Created,Admin,Active")] User user)
        {


            if (id != user.Login)
            {
                return NotFound();
            }
           
            if (ModelState.IsValid)
            {
                try
                {
                    var otc = await _context.Users.FindAsync(id);
                    if (null == otc)
                    {
                        return NotFound();
                    }
                    otc.Name = user.Name;
                    otc.Login = user.Login;
                    if (otc.Password != user.Password)
                        otc.Password = HashSh1(user.Password);
                    else otc.Password = user.Password;
                    otc.Created = user.Created;
                    otc.Admin = user.Admin;
                    otc.Active = user.Active;

                    _context.Update(otc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Login))
                    {
                        return NotFound();
                    }

                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = user.Login });
            }
            return View(user);
        }
        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Login == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'project_3Context.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexUsers));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Login == id);
        }
        public static bool UserAuthenticated(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.Session.GetString("username");
            if (user == null)
                return false;
            return true;
        }
        public static string? GetUser(IHttpContextAccessor accessor)
        {
            return accessor.HttpContext?.Session.GetString("username");

        }
        public static bool UserAuthenticatedAdmin(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.Session.GetString("username");
            if (user == null)
                return false;
            var admin = accessor.HttpContext?.Session.GetString("onadmin");
            if (admin == null || admin != "Admin")
                return false;
            return true;
        }


        // UTILS ------------------------------------->
        static string HashSh1(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hashSh1 = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

                // declare stringbuilder
                var sb = new StringBuilder(hashSh1.Length * 2);

                // computing hashSh1
                foreach (byte b in hashSh1)
                {
                    // "x2"
                    sb.Append(b.ToString("X2").ToLower());
                }

                // final output
                //Console.WriteLine(string.Format("The SHA1 hash of {0} is: {1}", input, sb.ToString()));

                return sb.ToString();
            }
        }
    }
}
