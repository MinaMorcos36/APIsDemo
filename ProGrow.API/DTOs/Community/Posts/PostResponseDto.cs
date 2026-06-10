namespace ProGrow.API.DTOs.Community.Posts
{
    public class PostResponseDto
    {
            public int Id { get; set; }
            public string Content { get; set; } = null!;
            public string? PostMediaUrl { get; set; }
            public DateTime CreatedAt { get; set; }

            public int? AuthorId { get; set; }
            public string? AuthorType { get; set; }

            public int LikesCount { get; set; }
            public int CommentsCount { get; set; }

            public bool IsLikedByMe { get; set; }
            public bool IsSavedByMe { get; set; }
            }
        }
