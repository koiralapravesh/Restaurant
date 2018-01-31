using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Restaurant.Models
{
    public class DynamicModel
    {
        public ObjectId Id { get; set; }
        public string field1 { get; set; }
        public string fiedl2 { get; set; }
        public double field01 { get; set; }
    }
}