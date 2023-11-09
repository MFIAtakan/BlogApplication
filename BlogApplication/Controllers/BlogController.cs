using BlogLab.Models.Blog;
using BlogLab.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;

namespace BlogApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogRepository blogRepository;
        private readonly IPhotoRepository photoRepository;

        public BlogController(IBlogRepository blogRepository, IPhotoRepository photoRepository)
        {
            this.blogRepository = blogRepository;
            this.photoRepository = photoRepository;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Blog>> CreateBlog(BlogCreate blogCreate)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);
            
            if(blogCreate.PhotoId.HasValue)
            {
                var photo = await photoRepository.GetAsync(blogCreate.PhotoId.Value);
                if(photo.ApplicationUserId!= applicationUserId)
                {
                    return BadRequest("You did not upload the photo.");
                }
            }

            var blog = await blogRepository.UpsertAsync(blogCreate, applicationUserId);
            return Ok(blog);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResults<Blog>>> GetAll([FromQuery] BlogPaging blogPaging)
        {
            var blogs = await blogRepository.GetAllAsync(blogPaging);
            return Ok(blogs);
        }

        [HttpGet("{blogId")]
        public async Task<ActionResult<Blog>> Get(int blogId)
        {
            var blog = await blogRepository.GetAsync(blogId);
            return Ok(blog);
        }

        [HttpGet("user/{applicationUserId}")]
        public async Task<ActionResult<List<Blog>>> GetByApplicationUserId(int applicationUserId)
        {
            var blogs = await blogRepository.GetAllByUserIdAsync(applicationUserId);
            return Ok(blogs);
        }


        [HttpGet("famous")]
        public async Task<ActionResult<List<Blog>>> GetAllFamous()
        {
            var blogs = await blogRepository.GetAllFamousAsync();
            return Ok(blogs);
        }

        [Authorize]
        [HttpDelete("{blogId}")]
        public async Task<ActionResult<int>> Delete(int blogId)
        {
            int applicationUserId = int.Parse(User.Claims.First(i => i.Type == JwtRegisteredClaimNames.NameId).Value);
            var foundBlog = await blogRepository.GetAsync(blogId);

            if(foundBlog == null)
            {
                return BadRequest("Blog does not exist");
            }
            if(foundBlog.ApplicationUserId == applicationUserId)
            {
                var affectedRows = await blogRepository.DeleteAsync(blogId);
                return Ok(affectedRows);
            }
            else
            {
                return BadRequest("You did not create this blog");
            }
        }
    }
}
