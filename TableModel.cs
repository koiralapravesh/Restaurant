using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Restaurant.Models
{
    public class TableModel
    {
        public ObjectId Id { get; set; }
        public string IdString
        {
            get
            {
                return Id.ToString();
            }
        }
        public int TableNumber { get; set; }
        public bool HelpRequested { get; set; }
        public OrderModel Order{ get; set; }
        public bool KitchenReady { get; set; } = false;
    }
}