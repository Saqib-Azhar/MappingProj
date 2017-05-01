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
    [Authorize(Roles = "Driver")]
    public class DriverDashboardController : Controller
    {
        
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private MappingDatabaseEntities db = new MappingDatabaseEntities();
        public DriverDashboardController()
        {
        }

        public DriverDashboardController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        
        // GET: DriverDashboard
        public ActionResult Index()
        {
            return View();
        }


        /*********************************************************************************/

        public ActionResult ViewMap()
        {
            var currentUser = User.Identity.GetUserId();

            var DriverVehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(x => x.DriverID == currentUser);
            int? id = DriverVehicleObj.VehicleID;
            var LocationObj = db.AspNetVehicleLocationTables.OrderByDescending(x => x.Id).FirstOrDefault(x => x.VehicleID == id);
            ViewBag.LastLatitude = LocationObj.LastLatitude;
            ViewBag.LastLongitude = LocationObj.LastLongitude;
            ViewBag.CarID = LocationObj.VehicleID;

            ViewBag.TimeStamp = LocationObj.TimeStamp;
            ViewBag.EngineRPM = LocationObj.EngineRPM;
            ViewBag.Speed = LocationObj.Speed;
            ViewBag.FuelPressure = LocationObj.FuelPressure;
            ViewBag.Throttle_Pos = LocationObj.Throttle_Pos;
            ViewBag.FuelType = LocationObj.FuelType;
            ViewBag.Fuel_Rail_Pressure = LocationObj.Fuel_Rail_Pressure;

            return View();
        }

        
        public ActionResult FleetHistory()
        {
            ViewBag.DriverID = User.Identity.GetUserId();
            return View();
        }



        public ActionResult Dashboard()
        {
            return View("BlankPage");
        }

        /***********************************************************************************/



    }
}