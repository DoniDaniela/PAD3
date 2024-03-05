using Service.Models;

namespace Service.Services
{
    public class MsBooksService
    {
        public async Task<List<MsBook>> GetAsync()
        {
            var list = new List<MsBook>();

            return list;
        }

        public async Task<MsBook?> GetAsync(string id)
        {
            return null;
        }

        public async Task CreateAsync(MsBook newBook)
        {
            //await _booksCollection.InsertOneAsync(newBook);
        }

        public async Task UpdateAsync(string id, MsBook updatedBook)
        {
            //await _booksCollection.ReplaceOneAsync(x => x.Id == id, updatedBook);
        }

        public async Task RemoveAsync(string id)
        {
            //await _booksCollection.DeleteOneAsync(x => x.Id == id);
        }

    }
}
