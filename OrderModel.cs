using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Restaurant.Models
{
    public class OrderModel
    {
        public ObjectId Id { get; set; }
        public string IdString { get { return Id.ToString(); } }
        public double total { get; set; }
        private DateTime _create;
        public DateTime createdAt
        {
            get
            {
                return _create.ToUniversalTime();
            }
            set
            {
                _create = value;
            }
        }
        public List<string> OrderItems { get; set; }
        public List<string> OrderMessages { get; set; }
        public double secondOffset
        {
            get
            {
                return (DateTime.UtcNow - createdAt).TotalSeconds;
            }
        }
    }
}