using Microsoft.Extensions.Options;
using Service.Models;
using MongoDB.Driver;

namespace Service.Services
{
    public class MongoBooksService
    {
        private readonly IMongoCollection<MongoBook> _booksCollection;

        public MongoBooksService(
            IOptions<BookStoreDatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _booksCollection = mongoDatabase.GetCollection<MongoBook>(
                bookStoreDatabaseSettings.Value.BooksCollectionName);
        }

        public async Task<List<MongoBook>> GetAsync() =>
            await _booksCollection.Find(_ => true).ToListAsync();

        public async Task<MongoBook?> GetAsync(string id) =>
            await _booksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(MongoBook newBook) =>
            await _booksCollection.InsertOneAsync(newBook);

        public async Task UpdateAsync(string id, MongoBook updatedBook) =>
            await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _booksCollection.DeleteOneAsync(x => x.Id == id);
    }
}
