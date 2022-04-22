using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Agnos;
using System.Web.Http;
using System.Collections.Generic;
using System.Web.Http.Results;
using System.Net;


namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var controller = new ProductsController();
            var res = controller.Get();
        }

        private List<Product> results()
        {
            var prods = new List<Product>();
            prods.Add(new Product { id = "626209e11484eb6966ec76b8", name = "Espresso", price = 3.5 });
            prods.Add(new Product { id = "62620a241484eb6966ec76b9", name = "cookie", price = 5 });
            prods.Add(new Product { id = "62620a441484eb6966ec76ba", name = "latte", price = 6.75 });
            prods.Add(new Product { id = "62622b6fd367d58f8823b3e4", name = "mocha", price = 6.5 });
            prods.Add(new Product { id = "6262dbaa785621011ccd9b52", name = "blt", price = 11 });

            return prods;
        }
    }
}
