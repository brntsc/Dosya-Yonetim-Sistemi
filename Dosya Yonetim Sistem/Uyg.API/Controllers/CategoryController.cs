using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;
using Microsoft.Extensions.Logging;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<FileModel> _fileRepository;
        private readonly ProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryController> _logger;
        ResultDto _result = new ResultDto();

        public CategoryController(IGenericRepository<Category> categoryRepository, IGenericRepository<FileModel> fileRepository, ProductRepository productRepository, IMapper mapper, ILogger<CategoryController> logger)
        {
            _categoryRepository = categoryRepository;
            _fileRepository = fileRepository;
            _productRepository = productRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("GetAll metodu çağrıldı");

                var categories = await _categoryRepository.Table
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                _logger.LogInformation($"Veritabanından {categories.Count} kategori bulundu");

                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
                
                _logger.LogInformation($"DTO'ya dönüştürülen kategori sayısı: {categoryDtos.Count}");
                
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler getirilirken bir hata oluştu");
                return StatusCode(500, new { message = "Kategoriler getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetById(id);
                if (category == null)
                    return NotFound(new { message = "Kategori bulunamadı" });

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kategori getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}/Products")]
        public async Task<List<ProductDto>> ProductList(int id)
        {
            try
            {
                var products = await _productRepository.GetAll();
                products = products.Where(s => s.CategoryId == id).ToList();
                
                var productDtos = _mapper.Map<List<ProductDto>>(products);

                if (!User.IsInRole("Admin"))
                {
                    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    productDtos = productDtos.Where(s => s.UserId == userId).ToList();
                }
                
                return productDtos;
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste döndür
                return new List<ProductDto>();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CategoryDto categoryDto)
        {
            try
            {
                if (categoryDto == null)
                    return BadRequest(new { message = "Kategori verisi boş olamaz" });

                if (string.IsNullOrWhiteSpace(categoryDto.Name))
                    return BadRequest(new { message = "Kategori adı boş olamaz" });

                var category = _mapper.Map<Category>(categoryDto);
                category.Created = DateTime.UtcNow;
                category.Updated = DateTime.UtcNow;
                category.IsActive = true;

                await _categoryRepository.Add(category);
                await _categoryRepository.Save();

                // Yeni oluşturulan kategoriyi döndür
                var createdCategory = await _categoryRepository.GetById(category.Id);
                var responseDto = _mapper.Map<CategoryDto>(createdCategory);
                
                return CreatedAtAction(nameof(GetById), new { id = category.Id }, responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kategori oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, CategoryDto categoryDto)
        {
            try
            {
                var category = await _categoryRepository.GetById(id);
                if (category == null)
                    return NotFound(new { message = "Kategori bulunamadı" });

                _mapper.Map(categoryDto, category);
                category.Updated = DateTime.Now;

                await _categoryRepository.Update(category);
                await _categoryRepository.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kategori güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _categoryRepository.GetById(id);
                if (category == null)
                    return NotFound(new { message = "Kategori bulunamadı" });

                // Kategoriye ait dosyaları kontrol et
                var files = await _fileRepository.Where(f => f.CategoryId == id).ToListAsync();
                if (files.Any())
                {
                    return BadRequest(new { message = "Bu kategoriye ait dosyalar var. Önce dosyaları silmelisiniz." });
                }

                await _categoryRepository.Delete(id);
                await _categoryRepository.Save();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kategori silinirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetCount")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _categoryRepository.Table.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Kategori sayısı alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetRecent")]
        public async Task<IActionResult> GetRecent()
        {
            try
            {
                var categories = await _categoryRepository.Table
                    .Include(c => c.Files)
                    .OrderByDescending(c => c.Created)
                    .Take(5)
                    .ToListAsync();

                // Debug için ham veriyi logla
                foreach (var cat in categories)
                {
                    Console.WriteLine($"Raw Category - ID: {cat.Id}, Name: {cat.Name}, Created: {cat.Created}");
                }

                var categoryDtos = _mapper.Map<List<CategoryDto>>(categories);
                
                // Her kategori için dosya sayısını ekle
                foreach (var category in categoryDtos)
                {
                    var categoryEntity = categories.First(c => c.Id == category.Id);
                    category.ProductCount = categoryEntity.Files?.Count ?? 0;
                    
                    // Debug için DTO verilerini logla
                    Console.WriteLine($"Mapped Category - ID: {category.Id}, Name: {category.Name}, ProductCount: {category.ProductCount}");
                }

                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecent Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Son kategoriler getirilirken bir hata oluştu: " + ex.Message });
            }
        }
    }
}
