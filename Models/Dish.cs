using System.ComponentModel.DataAnnotations;

namespace Mahmoud_Restaurant.Models
{
    public class Dish
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        public string Image { get; set; }

        public bool Vegetarian { get; set; }

        public double? Rating { get; set; }

        [Required]
        public DishCategory Category { get; set; }
    }

    public enum DishCategory
    {
        Wok,
        Pizza,
        Soup,
        Dessert,
        Drink
    }

}
