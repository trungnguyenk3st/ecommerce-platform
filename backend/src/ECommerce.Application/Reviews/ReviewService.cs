using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Reviews;

public class ReviewService : IReviewService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateReviewRequest> _validator;

    public ReviewService(IApplicationDbContext db, IValidator<CreateReviewRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<List<ReviewDto>> GetForProductAsync(int productId)
    {
        var reviews = await _db.Reviews.AsNoTracking()
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(ToDto).ToList();
    }

    public async Task<ReviewDto> CreateAsync(string userId, string userDisplayName, CreateReviewRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var alreadyReviewed = await _db.Reviews.AnyAsync(r => r.ProductId == request.ProductId && r.UserId == userId);
        if (alreadyReviewed)
            throw new DomainException("You have already reviewed this product.");

        var hasPurchased = await _db.Orders
            .Where(o => o.UserId == userId && o.Status != OrderStatus.PendingPayment && o.Status != OrderStatus.Cancelled)
            .SelectMany(o => o.Items)
            .AnyAsync(i => i.ProductId == request.ProductId);
        if (!hasPurchased)
            throw new DomainException("You can only review products you have purchased.");

        var review = new Review
        {
            ProductId = request.ProductId,
            UserId = userId,
            UserDisplayName = userDisplayName,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        await RecalculateProductRatingAsync(request.ProductId);

        return ToDto(review);
    }

    public async Task DeleteAsync(int reviewId)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId)
            ?? throw new NotFoundException(nameof(Review), reviewId);

        var productId = review.ProductId;
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        await RecalculateProductRatingAsync(productId);
    }

    private async Task RecalculateProductRatingAsync(int productId)
    {
        var product = await _db.Products.Include(p => p.Reviews).FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null) return;

        product.RecalculateRating();
        await _db.SaveChangesAsync();
    }

    private static ReviewDto ToDto(Review r) => new(r.Id, r.ProductId, r.UserDisplayName, r.Rating, r.Comment, r.CreatedAt);
}
