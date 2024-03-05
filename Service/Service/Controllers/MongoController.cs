using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Services;

namespace Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MongoController : ControllerBase
    {

        private readonly ILogger<MongoController> _logger;

        private readonly MongoBooksService _booksService;
        private readonly CacheService _cacheService;

        public MongoController(ILogger<MongoController> logger, MongoBooksService booksService, CacheService cacheService)
        {
            _logger = logger;
            _booksService = booksService;
            _cacheService = cacheService;
        }

        [HttpGet()]
        public async Task<List<MongoBook>> Get()
        {
            var books = _cacheService.GetData<List<MongoBook>>("mongo");
            if (books is null)
            {
                books = await _booksService.GetAsync();
                _cacheService.SetData("mongo", books);
            }
            return books;
        }

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<MongoBook>> Get(string id)
        {
            var book = _cacheService.GetData<MongoBook>(id);
            if (book is null)
            {
                book = await _booksService.GetAsync(id);
                if (book is null)
                {
                    return NotFound();
                }
                _cacheService.SetData(id, book);
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(MongoBook newBook)
        {
            await _booksService.CreateAsync(newBook);

            _cacheService.RemoveData("mongo");

            return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, MongoBook updatedBook)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedBook.Id = book.Id;

            await _booksService.UpdateAsync(id, updatedBook);

            _cacheService.RemoveData("mongo");
            _cacheService.SetData(id, updatedBook);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _booksService.RemoveAsync(id);

            _cacheService.RemoveData("mongo");
            _cacheService.RemoveData(id);

            return NoContent();
        }

    }
}
