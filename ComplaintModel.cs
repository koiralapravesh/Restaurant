using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Restaurant.Models
{
    public class ComplaintModel
    {
        public ObjectId Id { get; set; }
        public string IdString { get {
                try
                {
                    return Id.ToString();
                }
                catch (Exception)
                {
                    return "0000000000000";
                    throw;
                }
            } }
        public string Name { get; set; }
        public string Details { get; set; }
        public string Comp { get; set; }
        public DateTime Date { get; set; }
    }
}