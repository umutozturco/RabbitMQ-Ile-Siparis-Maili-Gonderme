using AutoMapper;
using OrderProcessingAPI.Models;

namespace OrderProcessingAPI.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Product, ProductDto>();
		}
	}
}
