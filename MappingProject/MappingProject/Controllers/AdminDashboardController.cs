using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MappingProject.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace MappingProject.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminDashboardController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private MappingDatabaseEntities db = new MappingDatabaseEntities();

        /*********************************************************************************/
        

        public AdminDashboardController()
        {
        }

        public AdminDashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /***************************************************************************************************************************0***/

        /***************************************************************************************************************************0***/

        public ActionResult Dashboard()
        {
            return View("BlankPage");
        }




        public ActionResult ManagerRegister()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManagerRegister(RegisterViewModel model)
        {
            var TransactionObj = db.Database.BeginTransaction();

            try
            {

            if (ModelState.IsValid)
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleStore = new RoleStore<IdentityRole>(context);
                    var roleManager = new RoleManager<IdentityRole>(roleStore);

                    var userStore = new UserStore<ApplicationUser>(context);
                    var userManager = new UserManager<ApplicationUser>(userStore);
                    userManager.AddToRole(user.Id, "Manager");
                    var CurrentUser = UserManager.FindById(User.Identity.GetUserId());

                    var ManagerObj = db.AspNetUsers.FirstOrDefault(x => x.Email == model.Email && x.UserName == model.UserName);
                    var AdminManagerObj = new AspNetAdmin_Managers();
                    AdminManagerObj.ManagerID = ManagerObj.Id;
                    AdminManagerObj.AdminID = CurrentUser.Id;
                    db.AspNetAdmin_Managers.Add(AdminManagerObj);
                    db.SaveChanges();

                    }
                    TransactionObj.Commit();
                    return RedirectToAction("ManagerIndex", "AspNetUsers");
                    
                    
                }

               // AddErrors(result);
            }
            catch(Exception ex)
            {
                TransactionObj.Dispose();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }




        public ActionResult DriverRegister()
        {
            ViewBag.ManagersList = new SelectList(db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Manager")), "Id", "UserName");
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DriverRegister(RegisterViewModel model)
        {
            var TransactionObj = db.Database.BeginTransaction();

            try
            {

                if (ModelState.IsValid)
                {
                    var SelectedManager = Request.Form["ManagersList"];
                    ApplicationDbContext context = new ApplicationDbContext();
                    var user = new ApplicationUser { UserName = model.UserName, Email = model.Email, PhoneNumber = model.PhoneNumber };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        var roleStore = new RoleStore<IdentityRole>(context);
                        var roleManager = new RoleManager<IdentityRole>(roleStore);

                        var userStore = new UserStore<ApplicationUser>(context);
                        var userManager = new UserManager<ApplicationUser>(userStore);
                        userManager.AddToRole(user.Id, "Driver");
                        var CurrentUser = UserManager.FindById(User.Identity.GetUserId());

                        var NewVehicle = new AspNetVehicle();
                        db.AspNetVehicles.Add(NewVehicle);
                        db.SaveChanges();


                        var DriverObj = db.AspNetUsers.FirstOrDefault(x => x.Email == model.Email && x.UserName == model.UserName);
                        var ManagerDriverObj = new AspNetManager_Drivers();
                        ManagerDriverObj.ManagerID = SelectedManager;
                        ManagerDriverObj.DriverID = DriverObj.Id;
                        db.AspNetManager_Drivers.Add(ManagerDriverObj);
                        db.SaveChanges();

                        var NewDriverVehicleObj = new AspNetDriver_Vehicle();
                        NewDriverVehicleObj.VehicleID = NewVehicle.VehicleID;
                        NewDriverVehicleObj.DriverID = DriverObj.Id;
                        db.AspNetDriver_Vehicle.Add(NewDriverVehicleObj);
                        db.SaveChanges();
                    }
                    TransactionObj.Commit();
                    return RedirectToAction("DriverIndex", "AspNetUsers");


                }

                // AddErrors(result);
            }
            catch (Exception ex)
            {
                TransactionObj.Dispose();
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }




        
            [Authorize]
        public ActionResult ViewFleetMap()
        {


            var managerDriverList = db.AspNetManager_Drivers.Select(s => s.ManagerID);

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


        /***************************************************************************************************************************0***/





        // GET: AdminDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}