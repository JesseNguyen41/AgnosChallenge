using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Agnos.Models
{
    public class Product
    {
        [BsonId]
        public ObjectId id;

        [BsonElement]
        public string name;

        [BsonElement]
        public double price; 
    }

    //could just have this be embedded inside the Product collection but don't want to have to pull this data every time we get the menu
    public class PriceAdjustment
    {
        [BsonId]
        public ObjectId id;

        [BsonElement]
        public string name;

        [BsonElement]
        public double discount;

        [BsonElement]
        public double taxRate;
    }

    public class CartItem
    {
        [BsonId]
        public ObjectId id;

        [BsonElement]
        public string name;

        [BsonElement]
        public string orderId;

        [BsonElement]
        public int quantity;

        [BsonElement]
        public double price;

        [BsonElement]
        public string customerName;
    }

    public class Order
    {
        [BsonId]
        public ObjectId id;

        [BsonElement]
        public double total;

        [BsonElement]
        public bool paid;

        [BsonElement]
        public bool ready;

        [BsonElement]
        public string customerName;

        [BsonElement]
        public List<string> items;

    }


}