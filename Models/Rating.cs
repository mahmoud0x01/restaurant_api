namespace Mahmoud_Restaurant.Models
{
    public class Rating
    {
        public Guid Id { get; set; }
        public Guid DishId { get; set; }
        public int UserId { get; set; } // User ID
        public int Score { get; set; } // Rating score (e.g., 1 to 5)
        public DateTime RatedAt { get; set; } // When the rating was made

    }
}
