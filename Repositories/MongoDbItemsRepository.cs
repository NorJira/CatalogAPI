using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Entities;
using Catalog.Settings;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
// using Catalog.Settings;

namespace Catalog.Repositories
{
  public class MongoDbItemsRepository : IItemsRepository
  {
    // private const string databaseName = "catalog";
    // private const string collectionName = "items";
    private readonly IMongoCollection<Item> itemsCollection;
    private readonly FilterDefinitionBuilder<Item> filterBuilder = Builders<Item>.Filter;

    public MongoDbItemsRepository(IMongoClient mongoClient, IConfiguration Configuration)
    {
      var settings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
      var dbName = settings.DbName;
      var dbCollection = settings.DbCollection;

      IMongoDatabase database = mongoClient.GetDatabase(dbName);
      itemsCollection = database.GetCollection<Item>(dbCollection);

      // IMongoDatabase database = mongoClient.GetDatabase(databaseName);
      // itemsCollection = database.GetCollection<Item>(collectionName);
    }

    public async Task CreateItemAsync(Item item)
    {
      await itemsCollection.InsertOneAsync(item);
    }

    public async Task DeleteItemAsync(Guid id)
    {
      var filter = filterBuilder.Eq( item => item.Id, id);
      await itemsCollection.DeleteOneAsync(filter);
    }

    public async Task<Item> GetItemAsync(Guid id)
    {
      var filter = filterBuilder.Eq( item => item.Id, id);
      return await itemsCollection.Find(filter).SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<Item>> GetItemsAsync()
    {
      return await itemsCollection.Find(new BsonDocument()).ToListAsync();
    }

    public async Task UpdateItemAsync(Item item)
    {
      var filter = filterBuilder.Eq( existingItem => existingItem.Id, item.Id);
      await itemsCollection.ReplaceOneAsync(filter, item);
    }
  }
}