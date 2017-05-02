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
            ViewBag.RegisterPage = "ManagerRegister";
            return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Manager")).ToList());


        }

        //public ViewResult DriverIndex()
        //{
        //    ViewBag.RegisterPage = "DriverRegister";
        //    return View("Index", db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Driver")).ToList());


        //}

        public ViewResult DriverIndex()
        {
            var managerDriverList = db.AspNetManager_Drivers.Select(s=>s.ManagerID);
           
            List<AspNetUser> UserList = new List<AspNetUser>();
            foreach (var item in managerDriverList)
            {
                var obj = db.AspNetUsers.FirstOrDefault(s => s.Id == item);
                var test = UserList.Find(s => s.Id == obj.Id);
                if (test == null)
                {
                    UserList.Add(obj);
                }
            }

            var list = new SelectList(UserList, "Id", "UserName");

            ViewBag.ManagerID = list;
            return View();

        }

        ////////////////////////////////////////////////////Common Functions///////////////////////////////////////////////////////////

        public class userdata
        {
            string UserName { set; get; }
            string Id { get; set; }
        }

        [HttpGet]
        public JsonResult DriversByManager(string id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetManager_Drivers> sub = db.AspNetManager_Drivers.Where(r => r.ManagerID == id).ToList();

            List<AspNetUser> list = new List<AspNetUser>();
            foreach(var item in sub)
            {
                var obj = db.AspNetUsers.FirstOrDefault(x => x.Id == item.DriverID);
                var test = list.Find(s => s.Id == obj.Id);
                if (test == null)
                {
                    list.Add(obj);
                }
            }

            var driversList = new SelectList(list, "Id", "UserName");
            return Json(driversList, JsonRequestBehavior.AllowGet);

        }

        //[HttpGet]
        //public JsonResult DriversIndexByManager(string id)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    List<AspNetManager_Drivers> sub = db.AspNetManager_Drivers.Where(r => r.ManagerID == id).ToList();

        //    List<AspNetUser> list = new List<AspNetUser>();
        //    foreach (var item in sub)
        //    {
        //        var obj = db.AspNetUsers.FirstOrDefault(x => x.Id == item.DriverID);
        //        var test = list.Find(s => s.Id == obj.Id);
        //        if (test == null)
        //        {
        //            list.Add(obj);
        //        }
        //    }
        //    //var HistoryLogs = (from log in db.AspNetVehicleLocationTables
        //    //                   where log.VehicleID == VehicleObj.VehicleID
        //    //                   select new { log.Id, log.LastLatitude, log.LastLongitude, log.Speed, log.Throttle_Pos, log.TimeStamp, log.VehicleID, log.EngineRPM, log.FuelPressure, log.FuelType, log.Fuel_Rail_Pressure }).ToList();



        //    var driversList = list;


        //    return Json(driversList, JsonRequestBehavior.AllowGet);

        //}

        public class DriverVehicle
        {
            public string UserName { get; set; }
            public string ID { get; set; }
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
            public int? VehicleID { get; set; }
        }


        [HttpGet]
        public JsonResult DriversIndexByManager(string id)
        {
            List<DriverVehicle> data = new List<DriverVehicle>();

            db.Configuration.ProxyCreationEnabled = false;
            List<AspNetManager_Drivers> sub = db.AspNetManager_Drivers.Where(r => r.ManagerID == id).ToList();


            foreach(var item in sub)
            {
                var driverobj = db.AspNetUsers.FirstOrDefault(x => x.Id == item.DriverID);
                var vehicleobj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == driverobj.Id);
                DriverVehicle drivervehicleObj = new DriverVehicle();
                drivervehicleObj.ID = driverobj.Id;
                drivervehicleObj.UserName = driverobj.UserName;
                drivervehicleObj.PhoneNumber = driverobj.PhoneNumber;
                drivervehicleObj.Email = driverobj.Email;
                drivervehicleObj.VehicleID = vehicleobj.VehicleID;

                var test = data.Find(s => s.ID == driverobj.Id);
                if (test == null)
                {
                    data.Add(drivervehicleObj);
                }
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /**********************************************************************************************+*****************************/

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
