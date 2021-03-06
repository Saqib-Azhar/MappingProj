﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MappingProject.Models;
using System.Text.RegularExpressions;

namespace MappingProject.Controllers
{
    public class AspNetVehicleLocationTablesController : Controller
    {
        private MappingDatabaseEntities db = new MappingDatabaseEntities();

        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public class ObdReading
        {
            public string vehicleid { get; set; }
            public double latitude { get; set; }
            public double longitude { get; set; }
            public int? timestamp { get; set; }
            public double altitude { get; set; }
            public readings readings { get; set; }
        }

        public class readings
        {
            public string FUEL_PRESSURE { get; set; }
            public string ENGINE_RPM { get; set; }
            public string SPEED { get; set; }
            public string THROTTLE_POS { get; set; }
            public string FUEL_TYPE { get; set; }
            public string ENGINE_LOAD { get; set; }
            public string FUEL_RAIL_PRESSURE { get; set; }

        }


        public void UpdateDB(ObdReading reading)
        {
            var vehicleobj = db.AspNetVehicles.FirstOrDefault(s => s.VehicleID == reading.vehicleid);
            var ReadingObj = new AspNetVehicleLocationTable();
            //var PreDecessorObj = new AspNetVehicleLocationTable();
            //try
            //{
            //    PreDecessorObj = db.AspNetVehicleLocationTables.OrderByDescending(x => x.Id).FirstOrDefault(x => x.VehicleID == Convert.ToInt32(reading.vehicleid));
            //}
            //catch { }
            //if (PreDecessorObj != null)
            //{
            //    int? TimeStampDiff = 0;
            //    var counter = PreDecessorObj.Id;
            //    var PredecessorCheckObj = new AspNetVehicleLocationTable();
            //    do
            //    {
            //        PredecessorCheckObj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == counter && s.VehicleID == Convert.ToInt32(reading.vehicleid) && s.TripStatus == "End");
            //        if (PredecessorCheckObj != null)
            //        {
            //            TimeStampDiff = reading.timestamp - PredecessorCheckObj.TimeStamp;
            //        }
            //        counter--;
            //    }
            //    while (PredecessorCheckObj == null && counter >= 0);
            //    if (TimeStampDiff > 90000 && PredecessorCheckObj != null)//here: 1000ms = 1s
            //    {
            //        ReadingObj.TripStatus = "Start";
            //    }
            //    else if(Convert.ToInt32(reading.readings.ENGINE_RPM) < 500)
            //    {
            //        PreDecessorObj.TripStatus = "OnRoad";
            //        db.SaveChanges();
            //        ReadingObj.TripStatus = "End";
            //    }
            //    else
            //    {
            //        ReadingObj.TripStatus = "OnRoad";
            //    }
            //}




            var PreDecessorObj = new AspNetVehicleLocationTable();
            try
            {
                PreDecessorObj = db.AspNetVehicleLocationTables.OrderByDescending(x => x.Id).FirstOrDefault(x => x.VehicleID == Convert.ToInt32(reading.vehicleid));
            }
            catch { }

            var counter = PreDecessorObj.Id;
            var PredecessorCheckObj = new AspNetVehicleLocationTable();
            if (PreDecessorObj != null)
            {
                do
                {
                    PredecessorCheckObj = db.AspNetVehicleLocationTables.OrderByDescending(x => x.Id).FirstOrDefault(s => s.Id == counter && s.VehicleID == Convert.ToInt32(reading.vehicleid) && s.TripStatus == "StartTrip" && Convert.ToInt32(reading.readings.ENGINE_RPM) >= 500);
                    if (PredecessorCheckObj != null)
                    {
                        ReadingObj.TripStatus = "OnTrip";
                    }
                    counter--;
                }
                while (PredecessorCheckObj == null && counter >= 0);
            }

            if (Convert.ToInt32(reading.readings.ENGINE_RPM) >= 500 && PredecessorCheckObj == null)
            {
                ReadingObj.TripStatus = "StartTrip";
            }
            else if (Convert.ToInt32(reading.readings.ENGINE_RPM) < 500 && PredecessorCheckObj == null)
            {
                ReadingObj.TripStatus = "EndTrip";
            }

            ReadingObj.LastLatitude = reading.latitude;
            ReadingObj.LastLongitude = reading.longitude;
            if (reading.readings.ENGINE_RPM == null)
            {
                reading.readings.ENGINE_RPM = "0";
            }
            else
            {
                ReadingObj.EngineRPM = Regex.Replace(reading.readings.ENGINE_RPM, "[A-Za-z ]", "");
                float engineRPM = Convert.ToInt32(ReadingObj.EngineRPM);
                ReadingObj.EngineRPM = Convert.ToString(engineRPM / 1000);
            }

            if (reading.readings.SPEED == null)
            {
                reading.readings.SPEED = "0";
            }
            else
            {
                ReadingObj.Speed = Regex.Replace(reading.readings.SPEED, "[A-Za-z ]", "");
                ReadingObj.Speed = Regex.Replace(ReadingObj.Speed, "/", "");
            }

            if (reading.readings.FUEL_PRESSURE == null)
            {
                reading.readings.FUEL_PRESSURE = "0";
            }
            else
            {
                ReadingObj.FuelPressure = Regex.Replace(reading.readings.FUEL_PRESSURE, "[A-Za-z ]", "");
            }

            if (reading.readings.THROTTLE_POS == null)
            {
                reading.readings.THROTTLE_POS = "0";
            }
            else
            {
                ReadingObj.Throttle_Pos = Regex.Replace(reading.readings.THROTTLE_POS, "[A-Za-z ]", "");
                ReadingObj.Throttle_Pos = Regex.Replace(reading.readings.THROTTLE_POS, "%", "");
            }

            if (reading.readings.FUEL_TYPE == null)
            {
                reading.readings.FUEL_TYPE = "0";
            }
            else
            {
                ReadingObj.FuelType = Regex.Replace(reading.readings.FUEL_TYPE, "[A-Za-z ]", "");
            }


            // ReadingObj.TimeStamp = Convert.ToInt32(reading.timestamp);
            ReadingObj.VehicleID = vehicleobj.Id;

            db.AspNetVehicleLocationTables.Add(ReadingObj);
            db.SaveChanges();

        }




        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public class TripRoute
        {
            public int RouteID { get; set; }
            public int RouteStartID { get; set; }
            public int RouteEndID { set; get; }
        }

        public JsonResult RouteHistory(string DriverID)
        {
            var vehicleDriverObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == DriverID);
            var id = vehicleDriverObj.VehicleID;

            int loopCounter = 0;
            int routeCounter = 1;
            int counter = 0;
            List<TripRoute> TripRouteList = new List<TripRoute>();
            var CounterObj = db.AspNetVehicleLocationTables.OrderByDescending(s => s.Id).FirstOrDefault(x => x.VehicleID == id && x.TripStatus == "EndTrip");
            loopCounter = CounterObj.Id;

            do
            {
                var RouteEnd = db.AspNetVehicleLocationTables.OrderByDescending(s => s.Id).FirstOrDefault(x => x.VehicleID == id && x.TripStatus == "EndTrip" && x.Id == loopCounter);
                if (RouteEnd != null)
                {
                    TripRoute route = new TripRoute();
                    route.RouteID = routeCounter;
                    route.RouteEndID = RouteEnd.Id;
                    AspNetVehicleLocationTable RouteStart = new AspNetVehicleLocationTable();
                    counter = RouteEnd.Id - 1;
                    do
                    {
                        RouteStart = db.AspNetVehicleLocationTables.OrderByDescending(x => x.Id).FirstOrDefault(s => s.Id == counter && s.VehicleID == id && s.TripStatus == "StartTrip");
                        counter--;
                    }
                    while (RouteStart == null && counter > 0);
                    if (RouteStart != null)
                    {
                        route.RouteStartID = RouteStart.Id;
                        TripRouteList.Add(route);
                        loopCounter = RouteStart.Id;
                        routeCounter++;
                        RouteStart = null;
                        RouteEnd = null;
                    }
                }
                loopCounter--;
            }
            while (loopCounter > 0);

            return Json(TripRouteList, JsonRequestBehavior.AllowGet);
        }



        public JsonResult GetLogObj(int LogID)
        {
            // var data = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == LogID);
            var data = (from sub in db.AspNetVehicleLocationTables
                        where sub.Id == LogID
                        select new { sub.EngineRPM, sub.FuelPressure, sub.FuelType, sub.Fuel_Rail_Pressure, sub.LastLatitude, sub.LastLongitude, sub.Speed, sub.Throttle_Pos, sub.TimeStamp, sub.VehicleID }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ViewRoute(int startid)
        {
            var locationObj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == startid);
            var VehicleDriverObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.VehicleID == locationObj.VehicleID);
            ViewBag.DriverID = VehicleDriverObj.DriverID;
            ViewBag.StartPt = startid;
            

            return View();
        }

        public class routeValues
        {
            
                public int vehicleid { get; set; }
                public double? latitude { get; set; }
                public double? longitude { get; set; }
                public string TripStatus { get; set; }

        }

        public JsonResult RouteLocations(int startid)
        {
            int RouteObjCounter = 0;
            var startObj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == startid);
            List<routeValues> RouteList = new List<routeValues>();
            RouteObjCounter = Convert.ToInt32(startObj.Id);
            int VehicleID = startObj.VehicleID;
            var routeobj = new AspNetVehicleLocationTable();
            do
            {
                routeobj = null;
                routeobj = db.AspNetVehicleLocationTables.FirstOrDefault(x => x.Id == RouteObjCounter && x.VehicleID == VehicleID);

                if (routeobj != null)
                {
                    routeValues routeValObj = new routeValues();
                    routeValObj.latitude = routeobj.LastLatitude;
                    routeValObj.longitude = routeobj.LastLongitude;
                    routeValObj.vehicleid = routeobj.VehicleID;
                    routeValObj.TripStatus = routeobj.TripStatus;
                    RouteList.Add(routeValObj);
                    routeValObj = null;
                }
                RouteObjCounter++;
            }
            while (routeobj == null|| routeobj.TripStatus != "EndTrip");


            return Json(RouteList,JsonRequestBehavior.AllowGet);
        }


        /***************************************************************************************************************************************************/
        public JsonResult OverSpeedDataByTrip(int startid)
        {
            var vehiclelocationobj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == startid);
            int RouteObjCounter = 0;
            var driverobj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.VehicleID == vehiclelocationobj.VehicleID);
            List<OverSpeed> data = new List<OverSpeed>();
            var x = 0;
            var a = 0;

            var vehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == driverobj.DriverID);

            //var overspeedPositions = (from sub in db.AspNetVehicleLocationTables
            //                          where sub.VehicleID == vehicleObj.VehicleID
            //                          select new { sub.Id, sub.LastLatitude, sub.Speed, sub.LastLongitude, sub.VehicleID, sub.EngineRPM }).ToList();

            List<AspNetVehicleLocationTable> overspeedPositions = new List<AspNetVehicleLocationTable>();

            RouteObjCounter = startid;
            var routeobj = new AspNetVehicleLocationTable();
            do
            {
                routeobj = null;
                routeobj = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == RouteObjCounter && s.VehicleID == vehiclelocationobj.VehicleID);

                if (routeobj != null)
                {
                    overspeedPositions.Add(routeobj);
                }
                RouteObjCounter++;
            }
            while (routeobj == null || routeobj.TripStatus != "EndTrip");

            

            foreach (var item in overspeedPositions)
            {
                OverSpeed overspeedobj = new OverSpeed();
                int counter = item.Id - 1;
                x = Convert.ToInt32(item.Speed);
                a = Convert.ToInt32(item.EngineRPM);
                if (x >= 50 && a >= 100)
                {
                    AspNetVehicleLocationTable LastLocation = null;
                    do
                    {
                        LastLocation = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == counter && s.VehicleID == item.VehicleID);
                        if (LastLocation != null)
                        {
                            var y = Convert.ToInt32(LastLocation.Speed);
                            if (y <= 49)
                            {
                                overspeedobj.latitude = item.LastLatitude;
                                overspeedobj.longitude = item.LastLongitude;
                                overspeedobj.vehicleid = item.VehicleID;
                                data.Add(overspeedobj);
                            }
                        }
                        counter--;
                    }
                    while (LastLocation == null);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);

        }




        /***************************************************************************************************************************************************/


        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



        [Authorize]
        public ActionResult ViewMap(int id)
        {
            try { 
            var LocationObj = db.AspNetVehicleLocationTables.OrderByDescending(x=>x.Id).FirstOrDefault(x=>x.VehicleID==id);
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
            }
            catch(Exception)
            { }
            return View();
        }

        
        public JsonResult MapData(string id)
        {
            var vehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == id);
            
            var data = (from sub in db.AspNetVehicleLocationTables
                        where sub.VehicleID == vehicleObj.VehicleID
                        orderby (sub.Id) descending
                        select new { sub.EngineRPM, sub.FuelPressure, sub.FuelType, sub.Fuel_Rail_Pressure, sub.LastLatitude, sub.LastLongitude, sub.Speed, sub.Throttle_Pos, sub.TimeStamp, sub.VehicleID }).FirstOrDefault();

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public class OverSpeed
        {
            public int vehicleid { get; set; }
            public double? latitude { get; set; }
            public double? longitude { get; set; }
        }

        public JsonResult OverSpeedData(string id)
        {
            List<OverSpeed> data = new List<OverSpeed>();
            var x = 0;
            var a = 0;
            
            var vehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == id);

            var overspeedPositions = (from sub in db.AspNetVehicleLocationTables
                        where sub.VehicleID == vehicleObj.VehicleID 
                        select new { sub.Id, sub.LastLatitude, sub.Speed, sub.LastLongitude, sub.VehicleID, sub.EngineRPM }).ToList();

            foreach(var item in overspeedPositions)
            {
                OverSpeed overspeedobj = new OverSpeed();
                int counter = item.Id - 1;
                x = Convert.ToInt32(item.Speed);
                a = Convert.ToInt32(item.EngineRPM);
                if (x >= 50 && a >= 100) 
                {
                    AspNetVehicleLocationTable LastLocation = null;
                    do
                    {
                        LastLocation = db.AspNetVehicleLocationTables.FirstOrDefault(s => s.Id == counter && s.VehicleID == item.VehicleID);
                        if (LastLocation != null)
                        {
                            var y = Convert.ToInt32(LastLocation.Speed);
                            if (y <= 49)
                            {
                                overspeedobj.latitude = item.LastLatitude;
                                overspeedobj.longitude = item.LastLongitude;
                                overspeedobj.vehicleid = item.VehicleID;
                                data.Add(overspeedobj);
                            }
                        }
                        counter--;
                    }
                    while (LastLocation == null);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);

        }
       
        [Authorize]
        public JsonResult UpdateView(int id)
        {
            var data = (from sub in db.AspNetVehicleLocationTables
                              where sub.VehicleID == id
                              orderby(sub.Id) descending
                        select new { sub.EngineRPM, sub.FuelPressure, sub.FuelType, sub.Fuel_Rail_Pressure, sub.LastLatitude, sub.LastLongitude, sub.Speed, sub.Throttle_Pos, sub.TimeStamp, sub.VehicleID }).FirstOrDefault();

           return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
       
        public ActionResult FleetHistory()
        {
            try { 
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
            }
            catch { }
            return View();
        }
        

            [HttpGet]
        public JsonResult FleetHistoryByDriver(string id)
        {
            ApplicationDbContext d = new ApplicationDbContext();

            db.Configuration.ProxyCreationEnabled = false;

            var VehicleObj = db.AspNetDriver_Vehicle.FirstOrDefault(s => s.DriverID == id);
            
            var HistoryLogs = (from log in db.AspNetVehicleLocationTables
                               where log.VehicleID == VehicleObj.VehicleID
                               select new { log.Id, log.LastLatitude, log.LastLongitude,log.Speed,log.Throttle_Pos,log.TimeStamp,log.VehicleID,log.EngineRPM,log.FuelPressure,log.FuelType,log.Fuel_Rail_Pressure }).ToList();

            return Json(HistoryLogs, JsonRequestBehavior.AllowGet);

        }


        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [Authorize]
        public ActionResult UpdateValues(int id)// To Manually update values of location //REMOVE IT AFTER APP CONFIGRATION
        {
           
            ViewBag.VehicleID = id;
            return View();
        }



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
