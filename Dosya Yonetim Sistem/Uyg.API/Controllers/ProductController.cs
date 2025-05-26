using System.Reflection;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Uyg.API.DTOs;
using Uyg.API.Models;
using Uyg.API.Repositories;

namespace Uyg.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _productRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ProductRepository productRepository, IMapper mapper, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<List<ProductDto>> List()
        {
            var products = await _productRepository.GetAll();
            var productDtos = _mapper.Map<List<ProductDto>>(products); 
            return productDtos;
        }

        [HttpGet("{id}")]
        public async Task<ProductDto> GetById(int id)
        {
            var product = await _productRepository.GetById(id);
            var productDto = _mapper.Map<ProductDto>(product);       
            return productDto;
        }

        [HttpPost]
        public async Task<ResultDto> Add(ProductDto model)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var product = _mapper.Map<Product>(model);
            product.Created = DateTime.Now;
            product.Updated = DateTime.Now;
            product.UserId = userId;
            product.PhotoUrl = "product.jpg";
            await _productRepository.Add(product);
            _result.Status = true;
            _result.Message = "Kayıt Eklendi";
            return _result;
        }
        [HttpPut]
        public async Task<ResultDto> Update(Product model)
        {
            var product = _mapper.Map<Product>(model);
            product.Updated = DateTime.Now;
            await _productRepository.Update(product);
            _result.Status = true;
            _result.Message = "Kayıt GüncelLendi";
            return _result;
        }

        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            await _productRepository.Delete(id);
            _result.Status = true;
            _result.Message = "Kayıt Silindi";
            return _result;
        }

  
        
        [Route("Upload")]
        [HttpPost]
      
        public async Task<ResultDto> Upload(ProductUploadDto dto)
        {
            var product = await _productRepository.GetById(dto.ProductId);
            if (product == null)
            {
                _result.Status = false;
                _result.Message = "Kayıt Bulunmadı!";
                return _result;
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot/Files/ProductPhotos");
            string productPic = product.PhotoUrl;

            if (productPic != "product.jpg")
            {

                var productPicUrl = Path.Combine(path, productPic);

                if (System.IO.File.Exists(productPicUrl))
                {
                    System.IO.File.Delete(productPicUrl);
                }
            }
            string data = dto.PicData;
            string base64 = data.Substring(data.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] imageBytes = Convert.FromBase64String(base64);
            string filePath = Guid.NewGuid().ToString() + dto.PicExt;


            var picPath = Path.Combine(path, filePath);

            System.IO.File.WriteAllBytes(picPath, imageBytes);

            product.PhotoUrl = filePath;
            await _productRepository.Update(product);

            _result.Status = true;
            _result.Message = "Ürün Fotoğrafı Güncellendi";

            return _result;
        }

    }
}
