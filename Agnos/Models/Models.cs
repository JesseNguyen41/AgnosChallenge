using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Agnos
{
    public class Product
    {
        [BsonId]
        public ObjectId id { get; set; }

        [BsonElement]
        public string name { get; set; }

        [BsonElement]
        public double price { get; set; }
    }

    //could just have this be embedded inside the Product collection but don't want to have to pull this data every time we get the menu
    public class PriceAdjustment
    {
        [BsonId]
        public ObjectId id { get; set; }

        [BsonElement]
        public string name { get; set; }

        [BsonElement]
        public double discount { get; set; }

        [BsonElement]
        public double taxRate { get; set; }
    }

    public class CartItem
    {
        [BsonId]
        public ObjectId id { get; set; }

        [BsonElement]
        public string name { get; set; }

        [BsonElement]
        public string orderId { get; set; }

        [BsonElement]
        public int quantity { get; set; }

        [BsonElement]
        public double price { get; set; }

        [BsonElement]
        public string customerName { get; set; }
    }

    public class Order
    {
        [BsonId]
        public ObjectId id { get; set; }

        [BsonElement]
        public double total { get; set; }

        [BsonElement]
        public bool paid { get; set; }

        [BsonElement]
        public bool ready { get; set; }

        [BsonElement]
        public string customerName { get; set; }

        [BsonElement]
        public List<string> items { get; set; }

    }


}