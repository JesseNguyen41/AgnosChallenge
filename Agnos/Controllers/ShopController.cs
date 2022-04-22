using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;

namespace Agnos.Controllers
{
    //Depending on the team development standards, I might have done dependency injection to decouple logic
    public class ProductsController : ApiController
    {
        public MongoClient client; 
        public IMongoDatabase db;
        public IMongoCollection<Models.Product> menu;
        public async Task<IHttpActionResult> Get()
        {
            client = new MongoClient("mongodb://127.0.0.1:27017");
            db = client.GetDatabase("CoffeeShop");

            menu = db.GetCollection<Models.Product>("Product");

            var res = await menu.Find(_ => true).ToListAsync();
            return Ok(res);
        }

        [Route("api/AddProduct")]
        public async Task<IHttpActionResult> Post([FromBody] Models.Product insertVal)
        {
            client = new MongoClient("mongodb://127.0.0.1:27017");
            db = client.GetDatabase("CoffeeShop");
            menu = db.GetCollection<Models.Product>("Product");
            await menu.InsertOneAsync(insertVal);
            return Ok();
        }
    }

    public class CartItemsController : ApiController
    {
        public MongoClient client;
        public IMongoDatabase db;
        public IMongoCollection<Models.Product> product;
        public IMongoCollection<Models.CartItem> cart;
        public IMongoCollection<Models.PriceAdjustment> adjust;
        public IMongoCollection<Models.Order> order;
        public async Task<IHttpActionResult> Post([FromBody] Models.CartItem item)
        {
            //Models.CartItem item = JsonConvert.DeserializeObject<Models.CartItem>(addToCart);
            client = new MongoClient("mongodb://127.0.0.1:27017");
            db = client.GetDatabase("CoffeeShop");
            adjust = db.GetCollection<Models.PriceAdjustment>("PriceAdjustment");
            var priceAdjustment = Builders<Models.PriceAdjustment>.Filter.Eq("name", item.name);
            Models.PriceAdjustment update = await adjust.Find(priceAdjustment).SingleAsync();

            product = db.GetCollection<Models.Product>("Product");
            var getProduct = Builders<Models.Product>.Filter.Eq("name", item.name);
            var prod = await product.Find(getProduct).SingleAsync();

            item.price = prod.price;
            double taxed = update.taxRate * item.price;

            //if the order has another item, the new added item might be discounted
            order = db.GetCollection<Models.Order>("Order");
            var findOrder = Builders<Models.Order>.Filter.Eq("orderId", item.orderId);
            Models.Order ord = await order.Find(findOrder).FirstOrDefaultAsync();
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
            cart = db.GetCollection<Models.CartItem>("CartItem");
            await cart.InsertOneAsync(item);

            if (ord != null)
            {
                var newList = ord.items;
                newList.Add(item.id.ToString());
                await order.UpdateManyAsync(findOrder, Builders<Models.Order>.Update.Set("total", ord.total += item.price).Set("items", newList)); 
            }
            else
            {
                Models.Order tempOrder = new Models.Order
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
    }

    public class OrdersController : ApiController
    {
        public MongoClient client;
        public IMongoDatabase db;
        public IMongoCollection<Models.Order> order;
        public IMongoCollection<Models.CartItem> cart;

        public async Task<IHttpActionResult> Get(string val)
        {
            client = new MongoClient("mongodb://localhost:27017");
            db = client.GetDatabase("CoffeeShop");

            var cartQuery = Builders<Models.CartItem>.Filter.Eq("orderId", val);
            cart = db.GetCollection<Models.CartItem>("CartItem");
            var eachItem = await cart.Find(cartQuery).ToListAsync();

            var OrderQuery = Builders<Models.Order>.Filter.Eq("id", val);
            order = db.GetCollection<Models.Order>("Order");
            var ord = await order.Find(OrderQuery).FirstAsync();
            var res = new { eachItem, total = ord.total };
            return Ok(res);
        }

        public async Task<IHttpActionResult> Put([FromBody] Models.Order submission)
        {

            var filter = Builders<Models.Order>.Filter.Eq("id", submission.id);
            var update = Builders<Models.Order>.Update.Set("paid", true).Set("ready", true);
            await order.UpdateManyAsync(filter, update);
            //After submitting order, notifies that order is ready after 4 seconds
            Thread.Sleep(4000);

            return Ok("Order is Ready");
            
        }
    }


}