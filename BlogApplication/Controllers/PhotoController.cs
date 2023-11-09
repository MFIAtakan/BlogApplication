using BlogLab.Models.Photo;
using BlogLab.Repository;
using BlogLab.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace BlogApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly PhotoRepository photoRepository;
        private readonly IBlogRepository blogRepository;
        private readonly IPhotoService photoService;

        public PhotoController(PhotoRepository photoRepository, IBlogRepository blogRepository, IPhotoService photoService)
        {
            this.photoRepository = photoRepository;
            this.blogRepository = blogRepository;
            this.photoService = photoService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Photo>> UploadPhoto(IFormFile file)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);

            var uploadResult = await photoService.AddPhotoAsync(file);
            if(uploadResult.Error!= null)
            {
                return BadRequest(uploadResult.Error.Message);
            }

            var photoCreate = new PhotoCreate
            {
                PublicId = uploadResult.PublicId,
                ImageUrl = uploadResult.SecureUrl.AbsoluteUri,
                Description = file.FileName
            };

            var photo = await photoRepository.InsertAsync(photoCreate, applicationUserId);
            return Ok(photo);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<Photo>>> GetByApplicationUserId()
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);
            var photos = photoRepository.GetAllByUserIdAsync(applicationUserId);
            return Ok(photos);
        }

        [HttpGet("{photoId}")]
        public async Task<ActionResult<Photo>> Get(int photoId)
        {
            var photo = await photoRepository.GetAsync(photoId);
            return Ok(photo);
        }

        [Authorize]
        [HttpDelete("{photoId}")]
        public async Task<ActionResult<int>> Delete(int photoId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);
            
            var foundPhoto =  await photoRepository.GetAsync(photoId);

            if (foundPhoto != null)
            {
                if(foundPhoto.ApplicationUserId == applicationUserId)
                {
                    var blogs = await blogRepository.GetAllByUserIdAsync(applicationUserId);
                    var usedInBlog = blogs.Any(b=> b.PhotoId == photoId);

                    if(usedInBlog)
                    {
                        return BadRequest("Can not remove photo as it is being used in publish blogs.");
                    }
                    var deleteResult = await photoService.DeletePhotoAsync(foundPhoto.PublicId);
                    if(deleteResult.Error != null)
                    {
                        return BadRequest(deleteResult.Error.Message);
                    }
                    var effectedRows = await photoRepository.DeleteAsync(foundPhoto.PhotoId);
                    return Ok(effectedRows);
                }
                return BadRequest("Photo was not uploaded by the current user.");
            }
            return BadRequest("Photo does not exist.");
        }

    }
}
