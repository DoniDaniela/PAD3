using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Modules;
using Service.Services;

namespace Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MsController : ControllerBase
    {
        private readonly ILogger<MsController> _logger;
        private readonly IConfiguration _confog;
        private readonly CacheService _cacheService;

        public MsController(ILogger<MsController> logger, IConfiguration config, CacheService cacheService)
        {
            _logger = logger;
            _confog = config;
            _cacheService = cacheService;
        }

        [HttpGet()]
        public async Task<List<MsBook>> Get()
        {
            var books = _cacheService.GetData<List<MsBook>>("ms");
            if (books is null)
            {
                books = DbUtils.GetBooks(_confog);
                _cacheService.SetData("ms", books);
            }
            return books;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MsBook>> Get(string id)
        {
            var book = _cacheService.GetData<MsBook>(id);
            if (book is null)
            {
                book = DbUtils.GetBook(_confog, id);
                if (book is null)
                {
                    return NotFound();
                }
                _cacheService.SetData(id, book);
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(MsBook newBook)
        {
            DbUtils.AddBook(_confog, newBook);

            _cacheService.RemoveData("ms");

            return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, MsBook updatedBook)
        {
            var book = DbUtils.GetBook(_confog, id);

            if (book is null)
            {
                return NotFound();
            }
            updatedBook.Id = book.Id;

            DbUtils.UpdateBook(_confog, id, updatedBook);

            _cacheService.RemoveData("ms");
            _cacheService.SetData(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = DbUtils.GetBook(_confog, id);

            if (book is null)
            {
                return NotFound();
            }

            DbUtils.DeleteBook(_confog, id);
            _cacheService.RemoveData("ms");
            _cacheService.RemoveData(id);

            return NoContent();
        }

    }
}
