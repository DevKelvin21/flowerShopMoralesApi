using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using flowerShopMoralesApi.Api.DTOs;
using flowerShopMoralesApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace flowerShopMoralesApi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [Authorize]
        [HttpPost("sale")]
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
