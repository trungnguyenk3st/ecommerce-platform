namespace ECommerce.Application.Reviews;

public interface IReviewService
{
    Task<List<ReviewDto>> GetForProductAsync(int productId);
    Task<ReviewDto> CreateAsync(string userId, string userDisplayName, CreateReviewRequest request);
    Task DeleteAsync(int reviewId);
}
