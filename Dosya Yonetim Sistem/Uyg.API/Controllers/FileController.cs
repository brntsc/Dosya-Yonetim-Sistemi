using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FileRepository _fileRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileController> _logger;
        ResultDto _result = new ResultDto();

        public FileController(
            FileRepository fileRepository, 
            IGenericRepository<Category> categoryRepository, 
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment,
            ILogger<FileController> logger)
        {
            _fileRepository = fileRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("GetAll metodu çağrıldı");

                var files = await _fileRepository.Table
                    .Include(f => f.Category)
                    .Include(f => f.FileTags)
                    .Include(f => f.User)
                    .Include(f => f.Comments)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.Created)
                    .ToListAsync();

                _logger.LogInformation($"Veritabanından {files.Count} dosya bulundu");

                var fileDtos = _mapper.Map<List<FileDto>>(files);
                
                _logger.LogInformation($"DTO'ya dönüştürülen dosya sayısı: {fileDtos.Count}");
                
                // Her bir dosya için detaylı loglama
                foreach (var fileDto in fileDtos)
                {
                    _logger.LogInformation($"Dosya ID: {fileDto.Id}, Ad: {fileDto.FileName}, Kategori: {fileDto.CategoryName}");
                }

                return Ok(fileDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosyalar getirilirken bir hata oluştu");
                return StatusCode(500, new { message = "Dosyalar getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var file = await _fileRepository.Where(f => f.Id == id)
                    .Include(f => f.Category)
                    .Include(f => f.FileTags)
                    .Include(f => f.User)
                    .FirstOrDefaultAsync();
                if (file == null)
                    return NotFound(new { message = "Dosya bulunamadı" });

                var fileDto = _mapper.Map<FileDto>(file);
                return Ok(fileDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Dosya getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            try
            {
                _logger.LogInformation("Upload metodu çağrıldı");
                _logger.LogInformation($"Gelen veri: File={dto.File?.FileName}, CategoryId={dto.CategoryId}, TagIds={string.Join(",", dto.TagIds ?? new List<int>())}");

                // Model validasyonu
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogError($"Model validation errors: {string.Join(", ", errors)}");
                    _logger.LogError($"ModelState Keys: {string.Join(", ", ModelState.Keys)}");
                    _logger.LogError($"ModelState Values: {string.Join(", ", ModelState.Values.Select(v => string.Join(", ", v.Errors.Select(e => e.ErrorMessage))))}");
                    return BadRequest(new { message = string.Join(", ", errors) });
                }

                // Dosya kontrolü
                if (dto.File == null)
                {
                    _logger.LogError("File is null");
                    return BadRequest(new { message = "Dosya seçilmedi" });
                }

                if (dto.File.Length == 0)
                {
                    _logger.LogError("File is empty");
                    return BadRequest(new { message = "Dosya boş olamaz" });
                }

                if (dto.File.Length > 104857600) // 100 MB
                {
                    _logger.LogError($"File size too large: {dto.File.Length} bytes");
                    return BadRequest(new { message = "Dosya boyutu 100 MB'dan büyük olamaz" });
                }

                // Kullanıcı kontrolü
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID not found in claims");
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                }

                // Uploads klasörünü kontrol et ve oluştur
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Uploads klasörü oluşturuldu: {uploadsFolder}");
                }

                // Dosya kaydetme
                var fileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
                var extension = Path.GetExtension(dto.File.FileName);
                var uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                _logger.LogInformation($"Saving file to: {filePath}");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                // Dosya modeli oluştur
                var fileModel = new FileModel
                {
                    FileName = dto.FileName ?? dto.File.FileName,
                    FilePath = uniqueFileName,
                    Description = dto.Description,
                    UploadDate = DateTime.Now,
                    UploadedBy = userId,
                    IsActive = true,
                    CategoryId = dto.CategoryId,
                    UserId = userId,
                    Created = DateTime.Now,
                    Updated = DateTime.Now
                };

                _logger.LogInformation($"Creating file model: {fileModel.FileName}");

                // Dosyayı kaydet
                await _fileRepository.Add(fileModel);
                await _fileRepository.Save();

                _logger.LogInformation($"File saved with ID: {fileModel.Id}");

                // Dosyayı tekrar çek (navigation property'ler için)
                var file = await _fileRepository.Where(f => f.Id == fileModel.Id)
                    .Include(f => f.Category)
                    .Include(f => f.FileTags)
                    .Include(f => f.User)
                    .FirstOrDefaultAsync();

                var fileDto = _mapper.Map<FileDto>(file);
                return Ok(fileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yüklenirken bir hata oluştu");
                _logger.LogError($"Exception details: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Dosya yüklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] FileUpdateDto dto)
        {
            try
            {
                _logger.LogInformation($"Update method called for file ID: {id}");
                _logger.LogInformation($"Incoming data - FileName: {dto.FileName}, Description: {dto.Description}, CategoryId: {dto.CategoryId}, TagIds: {(dto.TagIds != null ? string.Join(",", dto.TagIds) : "null")}");

                // Model validasyonu
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogError($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(new { message = string.Join(", ", errors) });
                }

                // Gelen verilerin kontrolü
                if (string.IsNullOrEmpty(dto.FileName?.Trim()))
                {
                    _logger.LogError("FileName boş olamaz");
                    return BadRequest(new { message = "Dosya adı boş olamaz" });
                }

                if (string.IsNullOrEmpty(dto.Description?.Trim()))
                {
                    _logger.LogError("Description boş olamaz");
                    return BadRequest(new { message = "Açıklama boş olamaz" });
                }

                if (dto.CategoryId <= 0)
                {
                    _logger.LogError($"Geçersiz CategoryId: {dto.CategoryId}");
                    return BadRequest(new { message = "Geçerli bir kategori seçilmelidir" });
                }

                // Kategori kontrolü - En başta yap
                var category = await _categoryRepository.Table
                    .Where(c => c.Id == dto.CategoryId && c.IsActive)
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    _logger.LogError($"Kategori bulunamadı veya aktif değil. CategoryId: {dto.CategoryId}");
                    return BadRequest(new { message = "Seçilen kategori bulunamadı veya aktif değil" });
                }

                // Dosyayı bul
                var file = await _fileRepository.Where(f => f.Id == id)
                    .Include(f => f.FileTags)
                    .FirstOrDefaultAsync();

                if (file == null)
                {
                    _logger.LogError($"Dosya bulunamadı. ID: {id}");
                    return NotFound(new { message = "Dosya bulunamadı" });
                }

                _logger.LogInformation($"Mevcut dosya bilgileri: FileName={file.FileName}, Description={file.Description}, CategoryId={file.CategoryId}");

                // Kullanıcı kontrolü
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID not found in claims");
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                var isAdmin = userRoles.Contains("Admin");

                if (file.UserId != userId && !isAdmin)
                {
                    _logger.LogError($"Yetkisiz erişim denemesi. User ID: {userId}, File User ID: {file.UserId}");
                    return Forbid();
                }

                // Yeni dosya yüklendiyse
                if (dto.File != null && dto.File.Length > 0)
                {
                    _logger.LogInformation($"Yeni dosya yükleniyor: {dto.File.FileName}, Boyut: {dto.File.Length} bytes");

                    // Eski dosyayı sil
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", file.FilePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        _logger.LogInformation($"Eski dosya siliniyor: {oldFilePath}");
                        System.IO.File.Delete(oldFilePath);
                    }
                    else
                    {
                        _logger.LogWarning($"Eski dosya bulunamadı: {oldFilePath}");
                    }

                    // Yeni dosyayı kaydet
                    var fileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
                    var extension = Path.GetExtension(dto.File.FileName);
                    var uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", uniqueFileName);

                    _logger.LogInformation($"Yeni dosya kaydediliyor: {filePath}");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.File.CopyToAsync(stream);
                    }

                    file.FilePath = uniqueFileName;
                    _logger.LogInformation($"Yeni dosya yolu: {uniqueFileName}");
                }

                // Dosya bilgilerini güncelle
                file.FileName = dto.FileName.Trim();
                file.Description = dto.Description.Trim();
                file.CategoryId = dto.CategoryId;
                file.Updated = DateTime.Now;

                _logger.LogInformation($"Güncellenen dosya bilgileri: FileName={file.FileName}, Description={file.Description}, CategoryId={file.CategoryId}");

                // Etiketleri güncelle
                if (dto.TagIds != null && dto.TagIds.Length > 0)
                {
                    _logger.LogInformation($"Etiketler güncelleniyor. TagIds: {string.Join(",", dto.TagIds)}");
                    file.FileTags.Clear();
                    foreach (var tagId in dto.TagIds)
                    {
                        var tag = await _fileRepository.GetFileTagById(tagId);
                        if (tag != null)
                        {
                            file.FileTags.Add(tag);
                            _logger.LogInformation($"Etiket eklendi: {tag.TagName}");
                        }
                        else
                        {
                            _logger.LogWarning($"Etiket bulunamadı: {tagId}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Tüm etiketler kaldırılıyor");
                    file.FileTags.Clear();
                }

                try
                {
                    _fileRepository.Update(file);
                    await _fileRepository.Save();
                    _logger.LogInformation($"Dosya başarıyla güncellendi. ID: {id}");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, $"Veritabanı güncelleme hatası. ID: {id}");
                    _logger.LogError($"Inner Exception: {ex.InnerException?.Message}");
                    _logger.LogError($"Stack Trace: {ex.StackTrace}");
                    return StatusCode(409, new { message = "Dosya başka bir kullanıcı tarafından güncellenmiş olabilir. Lütfen sayfayı yenileyip tekrar deneyin." });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"Veritabanı güncelleme hatası. ID: {id}");
                    _logger.LogError($"Inner Exception: {ex.InnerException?.Message}");
                    _logger.LogError($"Stack Trace: {ex.StackTrace}");
                    return StatusCode(500, new { message = "Veritabanı güncelleme hatası: " + ex.Message });
                }

                // Dosyayı tekrar çek (navigation property'ler için)
                file = await _fileRepository.Where(f => f.Id == id)
                    .Include(f => f.Category)
                    .Include(f => f.FileTags)
                    .Include(f => f.User)
                    .FirstOrDefaultAsync();

                var fileDto = _mapper.Map<FileDto>(file);
                return Ok(fileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya güncellenirken bir hata oluştu. ID: {id}");
                _logger.LogError($"Exception details: {ex.Message}");
                _logger.LogError($"Inner Exception: {ex.InnerException?.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Delete metodu çağrıldı. ID: {id}");

                // Token kontrolü
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User ID not found in claims");
                    return Unauthorized(new { message = "Kullanıcı bilgisi bulunamadı" });
                }

                var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                var isAdmin = userRoles.Contains("Admin");
                _logger.LogInformation($"User ID: {userId}, Is Admin: {isAdmin}, Roles: {string.Join(", ", userRoles)}");

                // Dosyayı bul
                var file = await _fileRepository.Where(f => f.Id == id)
                    .Include(f => f.FileTags)
                    .Include(f => f.Comments)
                    .Include(f => f.Category)
                    .FirstOrDefaultAsync();

                if (file == null)
                {
                    _logger.LogError($"Dosya bulunamadı. ID: {id}");
                    return NotFound(new { message = "Dosya bulunamadı" });
                }

                _logger.LogInformation($"Dosya bilgileri: ID={file.Id}, FileName={file.FileName}, CategoryId={file.CategoryId}, UserId={file.UserId}");

                // Yetki kontrolü
                if (file.UserId != userId && !isAdmin)
                {
                    _logger.LogError($"Yetkisiz erişim denemesi. User ID: {userId}, File User ID: {file.UserId}");
                    return Forbid();
                }

                // Fiziksel dosyayı sil
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", file.FilePath);
                _logger.LogInformation($"Fiziksel dosya yolu: {filePath}");

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                        _logger.LogInformation("Fiziksel dosya başarıyla silindi");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Fiziksel dosya silinirken hata oluştu: {filePath}");
                        return StatusCode(500, new { message = "Fiziksel dosya silinirken bir hata oluştu" });
                    }
                }

                // Veritabanından sil
                try
                {
                    await _fileRepository.Delete(id);
                    await _fileRepository.Save();
                    _logger.LogInformation($"Dosya başarıyla silindi. ID: {id}");
                    return Ok(new { message = "Dosya başarıyla silindi" });
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogError($"Dosya bulunamadı. ID: {id}");
                    return NotFound(new { message = "Dosya bulunamadı" });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"Veritabanı silme hatası. ID: {id}");
                    return StatusCode(500, new { message = "Dosya silinirken veritabanı hatası oluştu" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dosya silinirken bir hata oluştu. ID: {id}");
                return StatusCode(500, new { message = "Beklenmeyen bir hata oluştu" });
            }
        }

        [HttpGet("GetCount")]
        public async Task<IActionResult> GetCount()
        {
            try
            {
                var count = await _fileRepository.Table.CountAsync();
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya sayısı alınırken bir hata oluştu");
                return StatusCode(500, new { message = "Dosya sayısı alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetRecent")]
        public async Task<IActionResult> GetRecent()
        {
            try
            {
                _logger.LogInformation("GetRecent metodu çağrıldı");

                var files = await _fileRepository.Table
                    .Include(f => f.Category)
                    .Include(f => f.User)
                    .OrderByDescending(f => f.Created)
                    .Take(5)
                    .ToListAsync();

                _logger.LogInformation($"Veritabanından {files.Count} dosya bulundu");

                // Her dosya için detaylı loglama
                foreach (var file in files)
                {
                    _logger.LogInformation($"Raw File - ID: {file.Id}, Name: {file.FileName}, Category: {file.Category?.Name}, User: {file.User?.UserName}");
                }

                var fileDtos = _mapper.Map<List<FileDto>>(files);
                
                _logger.LogInformation($"DTO'ya dönüştürülen dosya sayısı: {fileDtos.Count}");
                
                // Her bir dosya için detaylı loglama
                foreach (var fileDto in fileDtos)
                {
                    _logger.LogInformation($"Mapped File - ID: {fileDto.Id}, Name: {fileDto.FileName}, Category: {fileDto.CategoryName}, User: {fileDto.UserName}");
                }

                return Ok(fileDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son dosyalar getirilirken bir hata oluştu");
                return StatusCode(500, new { message = "Son dosyalar getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetRecentCount")]
        public async Task<IActionResult> GetRecentCount()
        {
            try
            {
                _logger.LogInformation("GetRecentCount metodu çağrıldı");

                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var count = await _fileRepository.Table
                    .Where(f => f.Created >= thirtyDaysAgo)
                    .CountAsync();

                _logger.LogInformation($"Son 30 günde yüklenen dosya sayısı: {count}");

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son dosya sayısı alınırken bir hata oluştu");
                return StatusCode(500, new { message = "Son dosya sayısı alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetByCategory/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            try
            {
                _logger.LogInformation($"GetByCategory metodu çağrıldı. CategoryId: {categoryId}");

                var files = await _fileRepository.Table
                    .Include(f => f.Category)
                    .Include(f => f.User)
                    .Where(f => f.CategoryId == categoryId && f.IsActive)
                    .OrderByDescending(f => f.Created)
                    .ToListAsync();

                _logger.LogInformation($"Veritabanından {files.Count} dosya bulundu");

                var fileDtos = _mapper.Map<List<FileDto>>(files);
                
                _logger.LogInformation($"DTO'ya dönüştürülen dosya sayısı: {fileDtos.Count}");
                
                return Ok(fileDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriye göre dosyalar getirilirken bir hata oluştu");
                return StatusCode(500, new { message = "Kategoriye göre dosyalar getirilirken bir hata oluştu: " + ex.Message });
            }
        }
    }
} 