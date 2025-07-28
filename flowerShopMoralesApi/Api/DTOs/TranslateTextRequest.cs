using System;
using System.Text.Json;

namespace flowerShopMoralesApi.Api.DTOs;

public class TranslateTextRequest
{
    public string Prompt { get; set; } = string.Empty;
}

public class TranslateTextResponse
{
    public JsonDocument JsonPayload { get; set; }  = default!;
}
