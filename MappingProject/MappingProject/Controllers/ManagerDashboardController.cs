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
    [Authorize(Roles = "Manager")]
    public class ManagerDashboardController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private MappingDatabaseEntities db = new MappingDatabaseEntities();

        /*********************************************************************************/


        public ManagerDashboardController()
        {
        }

        public ManagerDashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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


        public ActionResult DriverRegister()
        {
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
                        var CheckVehicle = new AspNetVehicle();
                        var UserVehicleID = Request.Form["VehicleID"];
                        try {
                            CheckVehicle = db.AspNetVehicles.FirstOrDefault(s => s.VehicleID == UserVehicleID);
                            NewVehicle.Id = CheckVehicle.Id;
                            NewVehicle.VehicleID = CheckVehicle.VehicleID;
                        }
                        catch {
                        NewVehicle.VehicleID = UserVehicleID;
                        db.AspNetVehicles.Add(NewVehicle);
                        db.SaveChanges();
                        }
                        var DriverObj = db.AspNetUsers.FirstOrDefault(x => x.Email == model.Email && x.UserName == model.UserName);
                        var ManagerDriverObj = new AspNetManager_Drivers();
                        ManagerDriverObj.ManagerID = CurrentUser.Id;
                        ManagerDriverObj.DriverID = DriverObj.Id;
                        db.AspNetManager_Drivers.Add(ManagerDriverObj);
                        db.SaveChanges();

                        var NewDriverVehicleObj = new AspNetDriver_Vehicle();
                        NewDriverVehicleObj.VehicleID = NewVehicle.Id;
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
           
            return View(model);
        }




        /***************************************************************************************************************************0***/

        [Authorize]
        public ActionResult ViewFleetMap()
        {

            ViewBag.ManagerID = User.Identity.GetUserId();

            return View();
        }

        public ActionResult FleetHistory()
        {
            ViewBag.ManagerID = User.Identity.GetUserId();

            return View();
        }

        public ViewResult DriverIndex()
        {
            ViewBag.ManagerID = User.Identity.GetUserId();
            return View();

        }


        /***********************************************************************************************************************************/



        // GET: ManagerDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}