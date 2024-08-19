using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderProcessingAPI.Models;
using System;
using System.Linq;

namespace OrderProcessingAPI.Data
{
	public class DatabaseSeeder
	{
		public static void Seed(IServiceProvider serviceProvider)
		{
			using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

			if (!context.Products.Any())
			{
				var productFaker = new Faker<Product>()
					.RuleFor(p => p.Description, f => f.Commerce.ProductName())
					.RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
					.RuleFor(p => p.Unit, f => f.Commerce.ProductMaterial())
					.RuleFor(p => p.UnitPrice, f => f.Random.Decimal(1, 100))
					.RuleFor(p => p.Status, f => f.PickRandom("Available", "Unavailable"))
					.RuleFor(p => p.CreateDate, f => f.Date.Past())
					.RuleFor(p => p.UpdateDate, f => f.Date.Recent());

				var products = productFaker.Generate(1000);

				context.Products.AddRange(products);
				context.SaveChanges();
			}
		}
	}
}
