using MongoDB.Bson;
using MongoDB.Driver;
using Restaurant.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Restaurant.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var count = 1;
            var table_occupied_array = GetDB().GetCollection<TableModel>("Table").AsQueryable().Select(t => t.TableNumber).ToList();
            var open_table = 0;
            while (count <= 16)
            {
                if (!table_occupied_array.Contains(count))
                {
                    open_table = count;
                    break;
                }
                count++;
            }
            if (User.IsInRole("Admin"))
            {
                ViewBag.OpenTable = open_table;
                return View();
            }
            else
            {
                return RedirectToAction("Customer",new { table = open_table});
            }
        }
        public ActionResult Customer(string table)
        {
            Session["TableNumber"] = table;
            var db = GetDB();
            var coll = db.GetCollection<foodModel>("FoodMenu");
            var table_order = new TableModel();
            if (db.GetCollection<TableModel>("Table").AsQueryable().Any())
            {
                table_order = db.GetCollection<TableModel>("Table").AsQueryable().FirstOrDefault(t => t.TableNumber == int.Parse(table));
            }
            var food_list = new List<foodModel>();
            var food_items = coll.AsQueryable().ToList();
            var categories = food_items.Select(f => f.Category).Distinct().ToList();
            if (table_order != null && table_order.Order!=null)
            {
                food_list = db.GetCollection<foodModel>("FoodMenu").AsQueryable().Where(f => table_order.Order.OrderItems.Contains(f.Name)).ToList();
            }
            ViewBag.FoodItems = food_items;
            ViewBag.Categories = categories;
            ViewBag.Table = table;
            ViewBag.Current_Order = table_order?.Order;
            ViewBag.food_list = food_list;
            return View();
        }
        
       // [Authorize(Roles ="Admin")]
        public ActionResult TableGrid()
        {
            return View();
        }
        public ActionResult ViewComplaints()
        {

            ViewBag.first_20 = GetDB().GetCollection<ComplaintModel>("complaints").AsQueryable().OrderByDescending(c => c.Date).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult NeedHelp(string table)
        {
            int tn = (table != "N/A") ? int.Parse(table):0;
            var coll = GetDB().GetCollection<TableModel>("Table");
            if (coll.AsQueryable().Count(t => t.TableNumber == tn) > 0)
            {
                coll.UpdateOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, tn),
                                                                     Builders<TableModel>.Update.Set(t => t.HelpRequested, true));
            }
            else
            {
                coll.InsertOne(new TableModel
                {
                    TableNumber = tn,
                    HelpRequested = true
                });
            }
            return Json(new { response = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult HelpDone(int table)
        {
            GetDB().GetCollection<TableModel>("Table").UpdateOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, table),
                                                                 Builders<TableModel>.Update.Set(t => t.HelpRequested, false));
            return Json(new { response = true });
        }
        [HttpPost]
        public ActionResult AddOrder(List<string> order_ids,List<string> order_msg,double total,int tn)
        {
            var oids = order_ids.Select(i => new ObjectId(i)).ToList();
            var food_list = GetDB().GetCollection<foodModel>("FoodMenu").AsQueryable().Where(f => oids.Contains(f.Id)).Select(f => f.Name).ToList();
            var new_order = new OrderModel
            {
                total = total,
                OrderItems = food_list,
                OrderMessages = order_msg,
                createdAt = DateTime.UtcNow
            };
            GetDB().GetCollection<OrderModel>("orders").InsertOne(new_order);
            if (GetDB().GetCollection<TableModel>("Table").AsQueryable().Count(t => t.TableNumber == tn) > 0)
            {
                GetDB().GetCollection<TableModel>("Table").UpdateOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, tn),
                                                                     Builders<TableModel>.Update.Set(t => t.Order, new_order));
            }
            else
            {
                GetDB().GetCollection<TableModel>("Table").InsertOne(new TableModel
                {
                    TableNumber = tn,
                    Order = new_order
                });
            }
            return Json(new { response = true });
        }
        [HttpPost]
        public ActionResult cleanTable(int tn)
        {
            GetDB().GetCollection<TableModel>("Table").DeleteOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, tn));
            return Json(new { response = true });
        }
        [HttpPost]
        public ActionResult getUpdates(bool fromKitchen=false)
        {
            try
            {
                var stats = new List<TableModel>();
                if (fromKitchen) {
                    stats = GetDB().GetCollection<TableModel>("Table").AsQueryable().Where(t => t.HelpRequested != false || t.Order != null).Where(t=>t.KitchenReady==false).ToList();
                }
                else
                {
                    stats = GetDB().GetCollection<TableModel>("Table").AsQueryable().Where(t => t.HelpRequested != false || t.Order != null).ToList();
                }
                return Json(new { response = true, status = stats });
            }
            catch (Exception)
            {
                return Json(new { response = false });
            }
        }
        private IMongoDatabase GetDB()
        {
            var connectionString = ConfigurationManager.AppSettings["mongodbConnectionString"];
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("restaurant-csce4444");
            return database;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Entertainment()
        { 
            return View();
        }
        public ActionResult Kitchen()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReadyClicked(int tn)
        {
            GetDB().GetCollection<TableModel>("Table").UpdateOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, tn),
                                                                Builders<TableModel>.Update.Set(t => t.KitchenReady, true));
            return Json(new { resonse = true });
        }
        [HttpPost]
        public ActionResult UpdateOrder(int tn, List<string> names)
        {
            var table_order = GetDB().GetCollection<TableModel>("Table").AsQueryable().Where(t => t.TableNumber == tn).FirstOrDefault();
            if (table_order != null)
            {
                table_order.Order.OrderItems = table_order.Order.OrderItems.Where(o => !names.Contains(o)).ToList();
                var p_list =  GetDB().GetCollection<foodModel>("FoodMenu").AsQueryable().Where(f => table_order.Order.OrderItems.Contains(f.Name)).Select(f => new foodModel {Name=f.Name,Price= f.Price }).ToList();
                table_order.Order.total = table_order.Order.OrderItems.Select(o => p_list.Where(p => p.Name == o).Select(p => p.Price).First()).Sum();
            }
            GetDB().GetCollection<TableModel>("Table").UpdateOne(Builders<TableModel>.Filter.Eq(t => t.TableNumber, tn),
                                                                Builders<TableModel>.Update.Set(t => t.Order, table_order.Order));
            return Json(new { response = true });
        }
    }
}