namespace Parser.Entities;

public sealed class User
{
    /// <summary>
    /// Рейтинг продавца
    /// </summary>
    public double? TraderRating { get; }

    /// <summary>
    /// Количество отзывов продавца
    /// </summary>
    public int? ReviewsCount { get; }

    /// <summary>
    /// Количество закрытых объявлений продавцом
    /// </summary>
    public int? ClosedAds { get; }

    public User(double? traderRating, int? reviewsCount, int? closedAds)
    {
        TraderRating = traderRating;
        ReviewsCount = reviewsCount;
        ClosedAds = closedAds;
    }
}