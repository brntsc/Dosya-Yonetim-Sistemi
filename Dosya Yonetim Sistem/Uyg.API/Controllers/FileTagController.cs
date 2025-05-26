using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uyg.API.Data;
using Uyg.API.Models;
using Uyg.API.DTOs;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileTagController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FileTagController> _logger;

        public FileTagController(AppDbContext context, ILogger<FileTagController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileTag>>> GetAll()
        {
            try
            {
                _logger.LogInformation("GetAll metodu çağrıldı");
                var tags = await _context.FileTags.ToListAsync();
                _logger.LogInformation($"Toplam {tags.Count} etiket bulundu");
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Etiketler getirilirken bir hata oluştu");
                return StatusCode(500, new { message = "Etiketler getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileTag>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"GetById metodu çağrıldı. ID: {id}");
                var tag = await _context.FileTags.FindAsync(id);

                if (tag == null)
                {
                    _logger.LogWarning($"Etiket bulunamadı. ID: {id}");
                    return NotFound(new { message = "Etiket bulunamadı" });
                }

                return Ok(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Etiket getirilirken bir hata oluştu. ID: {id}");
                return StatusCode(500, new { message = "Etiket getirilirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<FileTag>> Create(FileTag tag)
        {
            try
            {
                _logger.LogInformation("Create metodu çağrıldı");
                _logger.LogInformation($"Gelen veri: TagName={tag.TagName}");

                if (string.IsNullOrEmpty(tag.TagName?.Trim()))
                {
                    _logger.LogWarning("TagName boş olamaz");
                    return BadRequest(new { message = "Etiket adı boş olamaz" });
                }

                tag.TagName = tag.TagName.Trim();
                _context.FileTags.Add(tag);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Etiket başarıyla oluşturuldu. ID: {tag.Id}");
                return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Etiket oluşturulurken bir hata oluştu");
                return StatusCode(500, new { message = "Etiket oluşturulurken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FileTag tag)
        {
            try
            {
                _logger.LogInformation($"Update metodu çağrıldı. ID: {id}");
                _logger.LogInformation($"Gelen veri: TagName={tag.TagName}");

                if (id != tag.Id)
                {
                    _logger.LogWarning($"ID uyuşmazlığı. Path ID: {id}, Body ID: {tag.Id}");
                    return BadRequest(new { message = "ID uyuşmazlığı" });
                }

                if (string.IsNullOrEmpty(tag.TagName?.Trim()))
                {
                    _logger.LogWarning("TagName boş olamaz");
                    return BadRequest(new { message = "Etiket adı boş olamaz" });
                }

                var existingTag = await _context.FileTags.FindAsync(id);
                if (existingTag == null)
                {
                    _logger.LogWarning($"Etiket bulunamadı. ID: {id}");
                    return NotFound(new { message = "Etiket bulunamadı" });
                }

                existingTag.TagName = tag.TagName.Trim();
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Etiket başarıyla güncellendi. ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Etiket güncellenirken bir hata oluştu. ID: {id}");
                return StatusCode(500, new { message = "Etiket güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Delete metodu çağrıldı. ID: {id}");

                var tag = await _context.FileTags.FindAsync(id);
                if (tag == null)
                {
                    _logger.LogWarning($"Etiket bulunamadı. ID: {id}");
                    return NotFound(new { message = "Etiket bulunamadı" });
                }

                _context.FileTags.Remove(tag);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Etiket başarıyla silindi. ID: {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Etiket silinirken bir hata oluştu. ID: {id}");
                return StatusCode(500, new { message = "Etiket silinirken bir hata oluştu: " + ex.Message });
            }
        }
    }
} 