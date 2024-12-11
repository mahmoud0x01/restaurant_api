using Microsoft.AspNetCore.Mvc;
using Mahmoud_Restaurant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Win32;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Mahmoud_Restaurant.Controllers
{
    public enum SortingCriteria
    {
        NameAsc,
        NameDesc,
        PriceAsc,
        PriceDesc,
        RatingAsc,
        RatingDesc
    }
    public enum DishCategory
    {
        Wok,
        Pizza,
        Soup,
        Dessert,
        Drink
    }

    public class DishQueryParams
    {
        /// <summary>
        /// List of categories to filter by
        /// </summary>
        [FromQuery(Name = "categories")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public List<DishCategory> Categories { get; set; } = new List<DishCategory>();

        /// <summary>
        /// Filter by vegetarian dishes
        /// </summary>
        [FromQuery(Name = "vegetarian")]
        public bool? Vegetarian { get; set; }

        /// <summary>
        /// Sorting criteria (e.g., NameAsc, NameDesc, PriceAsc, PriceDesc, RatingAsc, RatingDesc)
        /// </summary>

        [Required]
        [FromQuery(Name = "sorting")]
        [SwaggerSchema("Sorting criteria: NameAsc, NameDesc, PriceAsc, PriceDesc, RatingAsc, RatingDesc")]
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public SortingCriteria Sorting { get; set; } = SortingCriteria.NameAsc;


        /// <summary>
        /// Page number for pagination
        /// </summary>
        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;
    }



    [Route("api/[controller]")]
    [ApiController]
    public class DishController : ControllerBase
    {

        private readonly DishService _dishService;
        private readonly AuthService _authService;
        public DishController(DishService dishService, AuthService authService)
        {
            _dishService = dishService;
            _authService = authService;
        }
        // GET: api/<<Dish>>
        [HttpGet]
        public IActionResult GetDishes([FromQuery] DishQueryParams queryParams)
        {
            const int pageSize = 5;

            var sortingCriteria = queryParams.Sorting.ToString();
            var categories = queryParams.Categories?.Select(c => c.ToString()).ToList();

            var dishes = _dishService.GetFilteredDishes(categories, queryParams.Vegetarian, sortingCriteria, queryParams.Page, pageSize).ToList();

            int totalItems = _dishService.GetFilteredDishes(categories, queryParams.Vegetarian, sortingCriteria, 1, int.MaxValue).Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var response = new
            {
                Dishes = dishes,
                Pagination = new
                {
                    Size = pageSize,
                    Count = totalItems,
                    Current = queryParams.Page
                }
            };

            return Ok(response);
        }

        // GET api/<<Dish>>/{id}
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        // GET api/<Dish>/{id}/rating/check
        [HttpGet("{id}/rating/check")]
        public string Get_Check(int id)
        {
            return "value";
        }
        // POST api/<Dish>/{id}/rating
        [HttpPost("{id}/rating")]
        public void Post([FromBody] string value)
        {
        }
        [HttpPut] // PUT api/<Dish>/AddDish
        [Authorize] // Ensures the user is authenticated
        public async Task<IActionResult> AddDish([FromQuery] DishDto dishdto)
        {
            // Get the authenticated user's email
            var userEmail = User.Identity?.Name;
            if (userEmail == null)
                return Unauthorized("User is not authenticated.");

            // Check if the user is an admin
            var user = await _authService.Authorize(userEmail);
            if (!user.IsAdmin)
                return Unauthorized("Admin access is required to add dishes.");

            // Add the dish
            var dish = await _dishService.AddDishAsync(dishdto);
            var existingDish = await _dishService.GetDishByIdAsync(dish.Id);
            if (existingDish != null)
            {
                return CreatedAtAction(nameof(AddDish), new { id = dish.Id }, new { Name = dish.Name, Price = dish.Price });
            }
            else
            {
                return BadRequest("Dish was not created");
            }
        }

    }
}
