using Microsoft.AspNetCore.Mvc;
using Service.Models;
using Service.Modules;
using Service.Services;

namespace Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class twoPCController : ControllerBase
    {
        private readonly ILogger<twoPCController> _logger;
        private readonly IConfiguration _confog;
        private readonly MongoBooksService _booksService;

        public twoPCController(ILogger<twoPCController> logger, IConfiguration config, MongoBooksService booksService)
        {
            _logger = logger;
            _confog = config;
            _booksService = booksService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(MsBook newBook)
        {
            DbUtils.AddBothBook(_confog, newBook, () =>
            {
                try
                {
                    _ = _booksService.CreateAsync(new MongoBook()
                    {
                        BookName = newBook.BookName,
                        Author = newBook.Author,
                        Category = newBook.Category,
                        Price = newBook.Price
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                return false;
            });

            return Ok(newBook);
        }
    }
}
