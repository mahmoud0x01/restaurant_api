using System.ComponentModel.DataAnnotations;

namespace Mahmoud_Restaurant.Models
{
    public class DishDto
    {
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
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public DishCategory Category { get; set; }
    }


}
