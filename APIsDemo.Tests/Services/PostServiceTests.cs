using APIsDemo.DTOs.Community.Posts;
using APIsDemo.Models;
using APIsDemo.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace APIsDemo.Tests.Services
{
    public class PostServiceTests
    {
        private readonly AppDbContext _context;
        private readonly PostService _postService;

        public PostServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _postService = new PostService(_context);
        }

        [Fact]
        public async Task CreateAsync_ValidData_CreatesPostAndReturnsDto()
        {
            // Arrange
            var dto = new CreatePostDto
            {
                Content = "Hello World"
            };
            var authorId = 1;
            var authorType = "JobSeeker";

            // Act
            var result = await _postService.CreateAsync(dto, authorId, authorType);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello World", result.Content);
            Assert.Equal(authorId, result.AuthorId);
            Assert.Equal(authorType, result.AuthorType);
            Assert.True(result.Id > 0);

            var dbPost = await _context.Posts.FindAsync(result.Id);
            Assert.NotNull(dbPost);
            Assert.Equal("Hello World", dbPost.Content);
        }

        [Fact]
        public async Task GetFeedAsync_ReturnsPostsOrderedByDate()
        {
            // Arrange
            var post1 = new Post { Content = "Post 1", AuthorId = 1, AuthorType = "JobSeeker", CreatedAt = DateTime.UtcNow.AddMinutes(-10) };
            var post2 = new Post { Content = "Post 2", AuthorId = 2, AuthorType = "Recruiter", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };

            _context.Posts.Add(post1);
            _context.Posts.Add(post2);
            await _context.SaveChangesAsync();

            // Act
            var feed = await _postService.GetFeedAsync(1, "JobSeeker");

            // Assert
            Assert.NotNull(feed);
            Assert.Equal(2, feed.Count);
            // Verify order descending
            Assert.Equal("Post 2", feed[0].Content);
            Assert.Equal("Post 1", feed[1].Content);
        }
    }
}
