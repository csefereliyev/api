using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication8.Models;

namespace WebApplication8.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BooksController : ControllerBase
	{
		private static List<Book> _books = new List<Book>
		{
			new Book
			{
				Id = 1,
				Title = "Clean Code",
				Author = "Robert C. Martin",
				Category = "Proqramlaşdırma",
				PublishYear = 2008,
				PageCount = 464
			},
			new Book
			{
				Id = 2,
				Title = "1984",
				Author = "George Orwell",
				Category = "Bədii ədəbiyyat",
				PublishYear = 1949,
				PageCount = 328
			},
			new Book
			{
				Id = 3,
				Title = "The Pragmatic Programmer",
				Author = "Andrew Hunt",
				Category = "Proqramlaşdırma",
				PublishYear = 2019,
				PageCount = 352
			}
		};

		// GET /api/books - Bütün kitabları göstərmək
		[HttpGet]
		public ActionResult<IEnumerable<Book>> GetAllBooks()
		{
			return Ok(_books);
		}

		// GET /api/books/{id} - ID-yə görə kitabı tapmaq
		[HttpGet("{id}")]
		public ActionResult<Book> GetBookById(int id)
		{
			var book = _books.FirstOrDefault(b => b.Id == id);
			if (book == null)
			{
				return NotFound(new { message = $"ID-si {id} olan kitab tapılmadı." });
			}

			return Ok(book);
		}

		// POST /api/books - Yeni kitab yaratmaq
		[HttpPost]
		public ActionResult<Book> CreateBook([FromBody] Book newBook)
		{
			var validationError = ValidateBook(newBook);
			if (validationError != null)
			{
				return BadRequest(new { message = validationError });
			}

			newBook.Id = _books.Any() ? _books.Max(b => b.Id) + 1 : 1;
			_books.Add(newBook);

			return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
		}

		// PUT /api/books/{id} - Kitabı yeniləmək
		[HttpPut("{id}")]
		public IActionResult UpdateBook(int id, [FromBody] Book updatedBook)
		{
			var existingBook = _books.FirstOrDefault(b => b.Id == id);
			if (existingBook == null)
			{
				return NotFound(new { message = $"ID-si {id} olan kitab tapılmadı." });
			}

			var validationError = ValidateBook(updatedBook);
			if (validationError != null)
			{
				return BadRequest(new { message = validationError });
			}

			existingBook.Title = updatedBook.Title;
			existingBook.Author = updatedBook.Author;
			existingBook.Category = updatedBook.Category;
			existingBook.PublishYear = updatedBook.PublishYear;
			existingBook.PageCount = updatedBook.PageCount;

			return NoContent();
		}

		// DELETE /api/books/{id} - Kitabı silmək
		[HttpDelete("{id}")]
		public IActionResult DeleteBook(int id)
		{
			var book = _books.FirstOrDefault(b => b.Id == id);
			if (book == null)
			{
				return NotFound(new { message = $"ID-si {id} olan kitab tapılmadı." });
			}

			_books.Remove(book);
			return NoContent();
		}

		// ==========================================
		// 6. ƏLAVƏ TAPŞIRIQLAR (FİLTR VƏ AXTARIŞ)
		// ==========================================

		// 1. Kitabları adına görə axtarmaq
		[HttpGet("search")]
		public ActionResult<IEnumerable<Book>> SearchByTitle([FromQuery] string title)
		{
			if (string.IsNullOrWhiteSpace(title))
			{
				return BadRequest(new { message = "Axtarış üçün kitab adı daxil edin." });
			}

			var result = _books
				.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
				.ToList();

			return Ok(result);
		}

		// 2. Kitabları nəşr ilinə görə sıralamaq (asc və ya desc)
		[HttpGet("sorted")]
		public ActionResult<IEnumerable<Book>> SortByYear([FromQuery] string order = "asc")
		{
			var sorted = order.Equals("desc", StringComparison.OrdinalIgnoreCase)
				? _books.OrderByDescending(b => b.PublishYear).ToList()
				: _books.OrderBy(b => b.PublishYear).ToList();

			return Ok(sorted);
		}

		// 3. Yalnız 2000-ci ildən sonra nəşr olunan müasir kitabları göstərmək
		[HttpGet("modern")]
		public ActionResult<IEnumerable<Book>> GetModernBooks()
		{
			var modernBooks = _books.Where(b => b.PublishYear >= 2000).ToList();
			return Ok(modernBooks);
		}

		// 4. Kitabları kateqoriyasına görə filtrləmək
		[HttpGet("category/{category}")]
		public ActionResult<IEnumerable<Book>> GetByCategory(string category)
		{
			var filtered = _books
				.Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
				.ToList();

			return Ok(filtered);
		}

		// 5. Səhifə sayı 350-dən çox olan kitabları göstərmək
		[HttpGet("long")]
		public ActionResult<IEnumerable<Book>> GetLongBooks()
		{
			var longBooks = _books.Where(b => b.PageCount > 350).ToList();
			return Ok(longBooks);
		}

		// 6. Kitab məlumatlarını JSON faylı kimi ixrac (export) etmək
		[HttpGet("export")]
		public IActionResult ExportBooksJson()
		{
			var options = new JsonSerializerOptions { WriteIndented = true };
			var json = JsonSerializer.Serialize(_books, options);
			var bytes = Encoding.UTF8.GetBytes(json);

			return File(bytes, "application/json", "books_export.json");
		}

		// Biznes məntiqini yoxlayan köməkçi metod
		private string? ValidateBook(Book book)
		{
			if (book == null)
				return "Kitab məlumatları göndərilməyib.";

			if (string.IsNullOrWhiteSpace(book.Title))
				return "Kitabın adı boş ola bilməz.";

			if (book.PublishYear > DateTime.Now.Year)
				return $"Nəşr ili cari ildən ({DateTime.Now.Year}) böyük ola bilməz.";

			if (book.PageCount < 1)
				return "Səhifə sayı 1-dən az ola bilməz.";

			return null;
		}
	}
}