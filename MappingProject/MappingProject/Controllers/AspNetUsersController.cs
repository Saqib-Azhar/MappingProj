using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MappingProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MappingProject.Controllers
{
    [Authorize]
    public class AspNetUsersController : Controller
    {
        private MappingDatabaseEntities db = new MappingDatabaseEntities();
        /**********************************************************************************************************************/

        public ViewResult ManagerIndex()
        {
            return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Manager")).ToList());


        }
        
        public ViewResult DriverIndex()
        {
            return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Driver")).ToList());


        }

        /*****************************************************************************************************************************/
        // GET: AspNetUsers
        public ActionResult Index()
        {
            return View(db.AspNetUsers.ToList());
        }

        // GET: AspNetUsers/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // GET: AspNetUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AspNetUsers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.AspNetUsers.Add(aspNetUser);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(aspNetUser);
        }

        // GET: AspNetUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUsers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] AspNetUser aspNetUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(aspNetUser);
        }

        // GET: AspNetUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: AspNetUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {


            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            AspNetManager_Drivers ManagerDriverObj = db.AspNetManager_Drivers.FirstOrDefault(x => x.DriverID == id);
            
            AspNetDriver_Vehicle DriverVehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(x => x.DriverID == id);
            AspNetVehicle VehicleObj = db.AspNetVehicles.FirstOrDefault(x => x.VehicleID == DriverVehicleObj.VehicleID);



            ApplicationDbContext context = new ApplicationDbContext();
            var roleStore = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            userManager.RemoveFromRole(id, "Driver");

            db.AspNetUsers.Remove(aspNetUser);
            db.AspNetManager_Drivers.Remove(ManagerDriverObj);
            db.AspNetDriver_Vehicle.Remove(DriverVehicleObj);
            db.AspNetVehicles.Remove(VehicleObj);
            db.SaveChanges();

            //AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            //db.AspNetUsers.Remove(aspNetUser);
            //db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DriverDelete(string id)
        {


            return View();

        }

        [HttpPost, ActionName("DriverDelete")]
        public ActionResult DriverDeletePost(string id)
        {
            AspNetUser aspNetUser = db.AspNetUsers.Find(id);
            AspNetManager_Drivers ManagerDriverObj = db.AspNetManager_Drivers.FirstOrDefault(x => x.DriverID == id);
        
            AspNetDriver_Vehicle DriverVehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(x => x.DriverID == id) ;
            AspNetVehicle VehicleObj = db.AspNetVehicles.FirstOrDefault(x=>x.VehicleID==DriverVehicleObj.VehicleID);
           
            db.AspNetUsers.Remove(aspNetUser);
            db.AspNetManager_Drivers.Remove(ManagerDriverObj);
            db.AspNetDriver_Vehicle.Remove(DriverVehicleObj);
            db.AspNetVehicles.Remove(VehicleObj);
            db.SaveChanges();
            return RedirectToAction("Index");
        }








        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
