using System;
using System.Text.Json;

namespace flowerShopMoralesApi.Application.Interfaces;

public interface ITranslationService
{
    Task<JsonDocument> TranslatePromptToJsonAsync(string prompt);
}
