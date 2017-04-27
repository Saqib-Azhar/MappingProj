using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MappingProject.Models;

namespace MappingProject.Controllers
{
    public class AspNetVehicleLocationTablesController : Controller
    {
        private MappingDatabaseEntities db = new MappingDatabaseEntities();






        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class readings
        {
            public int vehicleid { get; set; }
            public float longitude { get; set; }
            public float latitude { get; set; }
            public float EngineRPM { get; set; }
            public float Speed { get; set; }
            public float FuelPressure { get; set; }
            public float Throttle_Pos { get; set; }
            public string FuelType { get; set; }
            public float Fuel_Rail_Pressure { set; get; }
            public string TimeStamp { get; set; }
        }

        [Authorize]
        public ActionResult ViewMap(int id)
        {
            var LocationObj = db.AspNetVehicleLocationTables.OrderByDescending(x=>x.Id).FirstOrDefault(x=>x.VehicleID==id);
            ViewBag.LastLatitude = LocationObj.LastLatitude;
            ViewBag.LastLongitude = LocationObj.LastLongitude;
            ViewBag.CarID = LocationObj.Id;

            ViewBag.TimeStamp = LocationObj.TimeStamp;
            ViewBag.EngineRPM = LocationObj.EngineRPM;
            ViewBag.Speed = LocationObj.Speed;
            ViewBag.FuelPressure = LocationObj.FuelPressure;
            ViewBag.Throttle_Pos = LocationObj.Throttle_Pos;
            ViewBag.FuelType = LocationObj.FuelType;
            ViewBag.Fuel_Rail_Pressure = LocationObj.Fuel_Rail_Pressure;

            return View();
        }

        [Authorize]
        public ActionResult UpdateValues(int id)// To Manually update values of location //REMOVE IT AFTER APP CONFIGRATION
        {
            var LocationObj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.VehicleID == id);
            ViewBag.LastLatitude = LocationObj.LastLatitude;
            ViewBag.LastLongitude = LocationObj.LastLongitude;
            ViewBag.VehicleID = LocationObj.Id;
            return View();
        }

        [Authorize]
        public void UpdateDB(readings reading)
        {
            var ReadingObj = new AspNetVehicleLocationTable();
            ReadingObj.LastLatitude = reading.latitude;
            ReadingObj.LastLongitude = reading.longitude;
            ReadingObj.EngineRPM = reading.EngineRPM;
            ReadingObj.Speed = reading.Speed;
            ReadingObj.FuelPressure = reading.FuelPressure;
            ReadingObj.Throttle_Pos = reading.Throttle_Pos;
            ReadingObj.FuelType = reading.FuelType;
            ReadingObj.TimeStamp = reading.TimeStamp;
            ReadingObj.VehicleID = reading.vehicleid;
            db.AspNetVehicleLocationTables.Add(ReadingObj);
            db.SaveChanges();

        }

        [Authorize]
        public JsonResult ViewUpdate(int id)
        {
            var LocationObj = db.AspNetVehicleLocationTables.LastOrDefault(x => x.VehicleID == id);
            return Json(LocationObj, JsonRequestBehavior.AllowGet);
        }

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////





        // GET: AspNetVehicleLocationTables
        public ActionResult Index()
        {
            var aspNetVehicleLocationTables = db.AspNetVehicleLocationTables.Include(a => a.AspNetVehicle);
            return View(aspNetVehicleLocationTables.ToList());
        }

        // GET: AspNetVehicleLocationTables/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetVehicleLocationTable aspNetVehicleLocationTable = db.AspNetVehicleLocationTables.Find(id);
            if (aspNetVehicleLocationTable == null)
            {
                return HttpNotFound();
            }
            return View(aspNetVehicleLocationTable);
        }

        // GET: AspNetVehicleLocationTables/Create
        public ActionResult Create()
        {
            ViewBag.VehicleID = new SelectList(db.AspNetVehicles, "VehicleID", "VehicleID");
            return View();
        }

        // POST: AspNetVehicleLocationTables/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,LastLatitude,LastLongitude,EngineRPM,Speed,FuelPressure,Throttle_Pos,FuelType,Fuel_Rail_Pressure,TimeStamp,VehicleID")] AspNetVehicleLocationTable aspNetVehicleLocationTable)
        {
            if (ModelState.IsValid)
            {
                db.AspNetVehicleLocationTables.Add(aspNetVehicleLocationTable);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.VehicleID = new SelectList(db.AspNetVehicles, "VehicleID", "VehicleID", aspNetVehicleLocationTable.VehicleID);
            return View(aspNetVehicleLocationTable);
        }

        // GET: AspNetVehicleLocationTables/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetVehicleLocationTable aspNetVehicleLocationTable = db.AspNetVehicleLocationTables.Find(id);
            if (aspNetVehicleLocationTable == null)
            {
                return HttpNotFound();
            }
            ViewBag.VehicleID = new SelectList(db.AspNetVehicles, "VehicleID", "VehicleID", aspNetVehicleLocationTable.VehicleID);
            return View(aspNetVehicleLocationTable);
        }

        // POST: AspNetVehicleLocationTables/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,LastLatitude,LastLongitude,EngineRPM,Speed,FuelPressure,Throttle_Pos,FuelType,Fuel_Rail_Pressure,TimeStamp,VehicleID")] AspNetVehicleLocationTable aspNetVehicleLocationTable)
        {
            if (ModelState.IsValid)
            {
                db.Entry(aspNetVehicleLocationTable).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.VehicleID = new SelectList(db.AspNetVehicles, "VehicleID", "VehicleID", aspNetVehicleLocationTable.VehicleID);
            return View(aspNetVehicleLocationTable);
        }

        // GET: AspNetVehicleLocationTables/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetVehicleLocationTable aspNetVehicleLocationTable = db.AspNetVehicleLocationTables.Find(id);
            if (aspNetVehicleLocationTable == null)
            {
                return HttpNotFound();
            }
            return View(aspNetVehicleLocationTable);
        }

        // POST: AspNetVehicleLocationTables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetVehicleLocationTable aspNetVehicleLocationTable = db.AspNetVehicleLocationTables.Find(id);
            db.AspNetVehicleLocationTables.Remove(aspNetVehicleLocationTable);
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
