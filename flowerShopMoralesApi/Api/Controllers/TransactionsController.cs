using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using flowerShopMoralesApi.Api.DTOs;
using flowerShopMoralesApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;


namespace flowerShopMoralesApi.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [Authorize]
        [HttpPost("sale", Name = "CreateSale")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleTransactionRequest request)
        {
            try
            {
                var id = await _transactionService.CreateSaleTransactionAsync(request);
                return CreatedAtAction(nameof(CreateSale), new { id }, new { transactionId = id });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
