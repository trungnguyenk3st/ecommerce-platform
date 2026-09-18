namespace ECommerce.Application.Reviews;

public record ReviewDto(int Id, int ProductId, string UserDisplayName, int Rating, string? Comment, DateTime CreatedAt);
public record CreateReviewRequest(int ProductId, int Rating, string? Comment);
