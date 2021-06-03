using MongoDB.Bson;
using MongoDB.Driver;
using OrderPlacer.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderConsumer.Services
{
	/// ARMAZENAMENTO DOS DADOS EM MONGODB
	public static class DataBaseService
	{
		//static List<Product> Products { get; set; }
		private static MongoClient client = new MongoClient();
		public static IMongoCollection<MongoDB.Bson.BsonDocument> Collection
		{
			get { return client.GetDatabase("test").GetCollection<BsonDocument>("estoque"); }
		}

		//Get um produto
		public static Order Get(int id)
		{
			var filter = Builders<BsonDocument>.Filter.Eq("id", id);
			var p = Collection.Find(filter).ToList()[0];
            var product = new Order
            {
                Id = p.GetValue("id").ToInt32(),
                Quantity = p.GetValue("quantity").ToInt32()
            };

            return product;
		}


		//Desconta produtos vendidos na loja
		public static void Discount(Order order)
		{
			var product = Get(order.Id);
			var filter = Builders<BsonDocument>.Filter.Eq("id", order.Id);
			var update = Builders<BsonDocument>.Update.Set("quantity", (product.Quantity - order.Quantity));
			Collection.UpdateOne(filter, update);
		}
	}
}
