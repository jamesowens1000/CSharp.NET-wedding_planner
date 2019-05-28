using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("Index");
        }

        [HttpPost("register")]
        public IActionResult TryRegister(IndexViewModel modelData)
        {
            User regUser = modelData.RegUser;
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == regUser.Email))
                {
                    ModelState.AddModelError("RegUser.Email", "Email already in use!");
                }
                else
                {
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    regUser.Password = Hasher.HashPassword(regUser, regUser.Password);
                    dbContext.Add(regUser);
                    dbContext.SaveChanges();
                    User currUser = dbContext.Users.FirstOrDefault(u => u.Email == regUser.Email);
                    HttpContext.Session.SetInt32("userId", regUser.UserId);
                    return RedirectToAction("Dashboard");
                }
            }
            return View("Index", modelData);
        }

        [HttpPost("login")]
        public IActionResult TryLogin(IndexViewModel modelData)
        {
            LoginUser logUser = modelData.LogUser;
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == logUser.Email);

                if (userInDb == null)
                {
                    ModelState.AddModelError("LogUser.Email", "Invalid Email/Password");
                }
                else
                {
                    var hasher = new PasswordHasher<LoginUser>();
                    var result = hasher.VerifyHashedPassword(logUser, userInDb.Password, logUser.Password);

                    if (result == 0)
                    {
                        ModelState.AddModelError("LogUser.Email", "Invalid Email/Password");
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("userId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            return View("Index", modelData);
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("userId") == null)
            {
                return RedirectToAction("Index");
            }

            List<Wedding> EveryWedding = dbContext.Weddings
                .Include(w => w.WeddingAttendees)
                .ThenInclude(a => a.User)
                .ToList();

            ViewBag.AllWeddings = EveryWedding;
            ViewBag.UserId = (int)HttpContext.Session.GetInt32("userId");
            return View("Dashboard");
        }

        [HttpGet("delete/{weddId}")]
        public IActionResult DeleteWedding(int weddId)
        {
            Wedding weddToDelete = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddId);
            dbContext.Weddings.Remove(weddToDelete);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("rsvp/{weddId}")]
        public IActionResult YesWedding(int weddId)
        {
            Attendance attendance = new Attendance();
            attendance.UserId = (int)HttpContext.Session.GetInt32("userId");
            attendance.WeddingId = weddId;
            dbContext.Attendances.Add(attendance);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("unrsvp/{attId}")]
        public IActionResult NoWedding(int attId)
        {
            Attendance attendance = dbContext.Attendances.FirstOrDefault(a => a.AttendanceId == attId);
            dbContext.Attendances.Remove(attendance);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("NewWedding")]
        public IActionResult NewWedding()
        {
            if (HttpContext.Session.GetInt32("userId") == null)
            {
                return RedirectToAction("Index");
            }
            return View("NewWedding");
        }

        [HttpPost("CreateWedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                newWedding.PlannerId = (int)HttpContext.Session.GetInt32("userId");
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                Wedding thisWedding = dbContext.Weddings.OrderByDescending(w => w.CreatedAt).FirstOrDefault();
                return Redirect("/Wedding/"+thisWedding.WeddingId);
            }
            return View("NewWedding", newWedding);
        }

        [HttpGet("Wedding/{weddId}")]
        public IActionResult WeddInfo(int weddId)
        {
            Wedding thisWedding = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddId);
            ViewBag.ThisWedding = thisWedding;

            var weddingGuests = dbContext.Weddings
                .Include(w => w.WeddingAttendees)
                .ThenInclude(u => u.User)
                .FirstOrDefault(w => w.WeddingId == weddId);
            
            ViewBag.AllGuests = weddingGuests.WeddingAttendees;
            return View("WeddInfo");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}