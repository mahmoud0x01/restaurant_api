﻿using Mahmoud_Restaurant.Data;
using Mahmoud_Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq;

public class DishService
{
    private readonly ApplicationDbContext _context;
    private readonly string _jwtSecret;
    private readonly string _adminSecretKey;
    private readonly ConcurrentDictionary<string, DateTime> _tokenBlacklist;
    // __init__vars
    public DishService(ApplicationDbContext context, string jwtSecret, string adminSecretKey, ConcurrentDictionary<string, DateTime> tokenBlacklist)
    { // __init__func
        _context = context;
        _jwtSecret = jwtSecret;
        _adminSecretKey = adminSecretKey;
        _tokenBlacklist = tokenBlacklist;
    }

    public IQueryable<Dish> GetFilteredDishes(List<string> categories, bool? vegetarian, string sorting, int page, int pageSize)
    {
        var dishesQuery = _context.Dishes.AsQueryable();

        // Filter by categories
        if (categories != null && categories.Any())
        {
            dishesQuery = dishesQuery.Where(d => categories.Contains(d.Category.ToString()));
        }

        // Filter by vegetarian
        if (vegetarian.HasValue)
        {
            dishesQuery = dishesQuery.Where(d => d.Vegetarian == vegetarian.Value);
        }

        // Sorting
        dishesQuery = sorting switch
        {
            "NameAsc" => dishesQuery.OrderBy(d => d.Name),
            "NameDesc" => dishesQuery.OrderByDescending(d => d.Name),
            "PriceAsc" => dishesQuery.OrderBy(d => d.Price),
            "PriceDesc" => dishesQuery.OrderByDescending(d => d.Price),
            "RatingAsc" => dishesQuery.OrderBy(d => d.Rating),
            "RatingDesc" => dishesQuery.OrderByDescending(d => d.Rating),
            _ => dishesQuery.OrderBy(d => d.Name)
        };

        // Pagination
        return dishesQuery.Skip((page - 1) * pageSize).Take(pageSize);
    }
    public async Task<Dish> AddDishAsync(DishDto dishDto)
    {
        var dish = new Dish
        {
            Name = dishDto.Name,
            Description = dishDto.Description,
            Image = dishDto.Image,
            Rating = dishDto.Rating,
            Price = dishDto.Price, // Assuming you want to cast double to decimal
            Category = dishDto.Category,
            Vegetarian = dishDto.Vegetarian
        };
        _context.Dishes.Add(dish);
        await _context.SaveChangesAsync();
        return dish;
    }
    public async Task<Dish> GetDishByIdAsync(Guid id)
    {
        return await _context.Dishes.FirstOrDefaultAsync(d => d.Id == id);
    }
}
