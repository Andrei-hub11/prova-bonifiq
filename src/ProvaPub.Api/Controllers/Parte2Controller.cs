﻿using Microsoft.AspNetCore.Mvc;
using ProvaPub.Contracts.Interfaces;
using ProvaPub.Models;
using ProvaPub.Repository;
using ProvaPub.Services;

namespace ProvaPub.Controllers
{

	[ApiController]
	[Route("[controller]")]
	public class Parte2Controller : ControllerBase
	{
		/// <summary>
		/// Precisamos fazer algumas alterações:
		/// 1 - Não importa qual page é informada, sempre são retornados os mesmos resultados. Faça a correção.
		/// 2 - Altere os códigos abaixo para evitar o uso de "new", como em "new ProductService()". Utilize a Injeção de Dependência para resolver esse problema
		/// 3 - Dê uma olhada nos arquivos /Models/CustomerList e /Models/ProductList. Veja que há uma estrutura que se repete. 
		/// Como você faria pra criar uma estrutura melhor, com menos repetição de código? E quanto ao CustomerService/ProductService. Você acha que seria possível evitar a repetição de código?
		/// 
		/// </summary>
		private readonly TestDbContext _ctx;
		private readonly ICustomerService _customerService;
		private readonly IProductService _productService;
		public Parte2Controller(TestDbContext ctx, ICustomerService customerService, IProductService productService)
		{
			_ctx = ctx;
			_customerService = customerService;
			_productService = productService;
		}

		[HttpGet("products")]
		public async Task<PaginatedResult<Product>> ListProducts(int page)
		{
			return await _productService.ListProducts(page);
		}

		/* [HttpGet("customers")]
		public async Task<CustomerList> ListCustomers(int page)
		{
			return await _customerService.ListCustomers(page);
		} */
	}
}
