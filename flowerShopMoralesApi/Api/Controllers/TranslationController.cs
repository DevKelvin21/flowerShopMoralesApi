using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using flowerShopMoralesApi.Api.DTOs;
using flowerShopMoralesApi.Application.Interfaces;

namespace flowerShopMoralesApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly ITranslationService _translationService;

    public TranslationController(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost("translate-text")]
    public async Task<ActionResult<TranslateTextResponse>> Translate([FromBody] TranslateTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt cannot be empty");

        var json = await _translationService.TranslatePromptToJsonAsync(request.Prompt);

        return Ok(new TranslateTextResponse { JsonPayload = json });
    }
}