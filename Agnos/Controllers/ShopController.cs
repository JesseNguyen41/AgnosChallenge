using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;

namespace Agnos
{
    //Depending on the team development standards, I might have done dependency injection to decouple logic
    public class ProductsController : ApiController
    {
        public MongoClient client; 
        public IMongoDatabase db;
        public IMongoCollection<Product> menu;
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                client = new MongoClient("mongodb://127.0.0.1:27017");
                db = client.GetDatabase("CoffeeShop");

                menu = db.GetCollection<Product>("Product");

                var res = await menu.Find(_ => true).ToListAsync();
                return Ok(res);
            }
            catch (Exception e)
            {
                using (StreamWriter writer = File.AppendText(@"C:\temp\AgnosLog.txt"))
                {

                    writer.WriteLine(e.Message + "\n");
                    writer.Flush();
                }
                return StatusCode(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/AddProduct")]
        public async Task<IHttpActionResult> Post([FromBody] Product insertVal)
        {
            try
            {
                client = new MongoClient("mongodb://127.0.0.1:27017");
                db = client.GetDatabase("CoffeeShop");
                menu = db.GetCollection<Product>("Product");
                await menu.InsertOneAsync(insertVal);
                return Ok();
            }
            catch (Exception e)
            {
                using (StreamWriter writer = File.AppendText(@"C:\temp\AgnosLog.txt"))
                {

                    writer.WriteLine(e.Message + "\n");
                    writer.Flush();
                }
                return StatusCode(System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }

    public class CartItemsController : ApiController
    {
        public MongoClient client;
        public IMongoDatabase db;
        public IMongoCollection<Product> product;
        public IMongoCollection<CartItem> cart;
        public IMongoCollection<PriceAdjustment> adjust;
        public IMongoCollection<Order> order;
        public async Task<IHttpActionResult> Post([FromBody] CartItem item)
        {
            try
            {
                client = new MongoClient("mongodb://127.0.0.1:27017");
                db = client.GetDatabase("CoffeeShop");
                adjust = db.GetCollection<PriceAdjustment>("PriceAdjustment");
                var priceAdjustment = Builders<PriceAdjustment>.Filter.Eq("name", item.name);
                PriceAdjustment update = await adjust.Find(priceAdjustment).SingleAsync();

                product = db.GetCollection<Product>("Product");
                var getProduct = Builders<Product>.Filter.Eq("name", item.name);
                var prod = await product.Find(getProduct).SingleAsync();

                item.price = prod.price;
                double taxed = update.taxRate * item.price;

                //if the order has another item, the new added item might be discounted
                order = db.GetCollection<Order>("Order");
                var findOrder = Builders<Order>.Filter.Eq("orderId", item.orderId);
                Order ord = await order.Find(findOrder).FirstOrDefaultAsync();
                if (ord != null)
                {
                    item.price = (1 - update.discount) * item.price + taxed;
                    //if discounting and consideration for quantity was better defined I would add more logic here to include quantity
                }
                else
                {
                    item.price += taxed;
                    //preferably I would have the front end generate the objectId based on user session
                    //but I could also make generate object Id here and use it for both the cart and order document

                }

                //might have adjusted pricing for quantity here too
                cart = db.GetCollection<CartItem>("CartItem");
                await cart.InsertOneAsync(item);

                if (ord != null)
                {
                    var newList = ord.items;
                    newList.Add(item.id.ToString());
                    await order.UpdateManyAsync(findOrder, Builders<Order>.Update.Set("total", ord.total += item.price).Set("items", newList));
                }
                else
                {
                    Order tempOrder = new Order
                    {
                        id = ObjectId.Parse(item.orderId),
                        total = item.price,
                        paid = false,
                        ready = false,
                        customerName = item.customerName,
                        items = new List<string>() { item.id.ToString() }
                    };
                    await order.InsertOneAsync(tempOrder);
                }
                return Ok();
            }
            catch (Exception e)
            {
                using (StreamWriter writer = File.AppendText(@"C:\temp\AgnosLog.txt"))
                {

                    writer.WriteLine(e.Message + "\n");
                    writer.Flush();
                }
                return StatusCode(System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }

    public class OrdersController : ApiController
    {
        public MongoClient client;
        public IMongoDatabase db;
        public IMongoCollection<Order> order;
        public IMongoCollection<CartItem> cart;

        public async Task<IHttpActionResult> Get(string orderId)
        {
            try
            {
                client = new MongoClient("mongodb://localhost:27017");
                db = client.GetDatabase("CoffeeShop");

                var cartQuery = Builders<CartItem>.Filter.Eq("orderId", orderId);
                cart = db.GetCollection<CartItem>("CartItem");
                var eachItem = await cart.Find(cartQuery).ToListAsync();

                var OrderQuery = Builders<Order>.Filter.Eq("id", ObjectId.Parse(orderId));
                order = db.GetCollection<Order>("Order");
                var ord = await order.Find(OrderQuery).FirstAsync();
                var res = new { eachItem, total = ord.total };
                return Ok(res);
            }
            catch (Exception e)
            {
                using (StreamWriter writer = File.AppendText(@"C:\temp\AgnosLog.txt"))
                {

                    writer.WriteLine(e.Message + "\n");
                    writer.Flush();
                }
                return StatusCode(System.Net.HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/Paid")]
        public async Task<IHttpActionResult> Put([FromBody] string submission)
        {
            try
            {
                client = new MongoClient("mongodb://localhost:27017");
                db = client.GetDatabase("CoffeeShop");
                order = db.GetCollection<Order>("Order");
                var filter = Builders<Order>.Filter.Eq("id", ObjectId.Parse(submission));

                var update = Builders<Order>.Update.Set("paid", true).Set("ready", true);
                await order.UpdateManyAsync(filter, update);
                //After submitting order, notifies that order is ready after 4 seconds
                Thread.Sleep(4000);

                return Ok("Order is Ready");
            }
            catch (Exception e)
            {
                using (StreamWriter writer = File.AppendText(@"C:\temp\AgnosLog.txt"))
                {

                    writer.WriteLine(e.Message + "\n");
                    writer.Flush();
                }
                return StatusCode(System.Net.HttpStatusCode.InternalServerError);
            }

        }
    }


}