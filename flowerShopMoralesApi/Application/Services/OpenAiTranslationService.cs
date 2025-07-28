using System;
using System.Text.Json;
using OpenAI.Chat;
using Google.Cloud.SecretManager.V1;
using flowerShopMoralesApi.Application.Interfaces;


namespace flowerShopMoralesApi.Application.Services;

public class OpenAiTranslationService : ITranslationService
{
    private readonly ChatClient _openAiClient;

    public OpenAiTranslationService(IConfiguration config)
    {
        string? apiKey;
        var environment = config["ASPNETCORE_ENVIRONMENT"];
        if (environment == "Development")
        {
            apiKey = config["OpenAI:ApiKey"];
        }
        else
        {
            var secretClient = SecretManagerServiceClient.Create();
            var secretName = new SecretVersionName("flowershop-morales", "openai-api-key", "latest");
            var result = secretClient.AccessSecretVersion(secretName);
            apiKey = result.Payload.Data.ToStringUtf8();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is missing");
        }
        _openAiClient = new("gpt-4o", apiKey);
    }

    public async Task<JsonDocument> TranslatePromptToJsonAsync(string prompt)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(
                "You are an assistant that extracts structured sales and expenses data from flower shop messages written in informal Spanish.\n\n" +
                "Each message may include free-text descriptions of sales (e.g. sold items) or expenses (e.g. purchases or operational costs). Messages may contain spelling mistakes or lack formal structure.\n\n" +
                "Output a valid JSON object with this structure:\n" +
                "{\n" +
                "  \"total_sale_price\": float,\n" +
                "  \"payment_method\": \"cash\" | \"bank_transfer\"\n" +
                "  \"sales\": [\n" +
                "    {\n" +
                "      \"item\": \"string\",\n" +
                "      \"quantity\": int or null,\n" +
                "      \"unit_price\": float or null,\n" +
                "      \"quality\": \"regular\" | \"special\"\n" +
                "    }\n" +
                "  ],\n" +
                "  \"expenses\": [\n" +
                "    {\n" +
                "      \"description\": \"string\",\n" +
                "      \"amount\": float\n" +
                "    }\n" +
                "  ],\n" +
                "}\n\n" +
                "\nExample:\n" +
                "Input: vendi un ramo de 12 rosas total $24 transferencia\n" +
                "Output:\n" +
                "{\n" +
                "  \"total_sale_price\": 24,\n" +
                "  \"payment_method\": \"bank_transfer\",\n" +
                "  \"sales\": [\n" +
                "    {\n" +
                "      \"item\": \"rosa\",\n" +
                "      \"quantity\": 12,\n" +
                "      \"unit_price\": 2,\n" +
                "      \"quality\": \"regular\"\n" +
                "    }\n" +
                "  ],\n" +
                "  \"expenses\": []\n" +
                "}\n" +
                "\n" +
                "Interpretation rules:\n" +
                "- If the message refers to selling products (e.g., \"ramo\", \"rosa\", \"bon\", \"oasis\", \"listón\") or is not clearly defined, classify as a sale.\n" +
                "- If the message refers to purchases or costs (e.g., \"compramos\", \"gastamos\"), classify as expenses.\n" +
                "- Always extract as many individual sale items as possible.\n" +
                "- Always extract items as singular (e.g., \"rosas\" -> \"rosa\").\n" +
                "- If the message includes \"x \" before the price then treat it as a total sale price unless \"cada uno\" or \"cada una\" is present\n" +
                "- Treat \"cada uno\" or \"cada una\" as unit price references.\n" +
                "- Handle combined items (e.g., \"ramo 12 rosas y 12 chocolates bon $19\") as a single line item.\n" +
                "- If quantity is not clear but price per unit is mentioned, infer quantity if possible.\n" +
                "- If unit price is ambiguous, leave it null.\n" +
                "- If the total_sale_price is null but the unit price and qty are available then calculate\n" +
                "- If the message includes \"total\" or \"total venta\", treat it as total sale price.\n" +
                "- Always try to extract the total sale price, even if it's not explicitly mentioned since users will mention explicitly if it's unit_price\n" +
                "- Accept and normalize product names beyond just flowers, including party gifts or accessories (e.g., chocolates, listón, oasis, varitas, claveles, helechos).\n" +
                "- If quality is not mentioned, default to \"regular\".\n" +
                "- If quality indicators such as \"de ecuador\", \"especial\", or \"premium\" appear, assign quality as \"special\".\n" +
                "- If indicators like \"de guatemala\", \"chapina\", \"de paquete\", or nothing are mentioned, assign \"regular\".\n" +
                "- If no payment method is mentioned, always default to \"cash\" and never return null." +
                "- Always return only valid JSON — no explanation, no commentary."
            ),
            new UserChatMessage(prompt)
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "flower_shop_transaction",
                jsonSchema: BinaryData.FromString("""
                {
                  "type": "object",
                  "properties": {
                    "total_sale_price": { "type": ["number"] },
                    "payment_method": { "type": ["string"], "enum": ["cash", "bank_transfer"] },
                    "sales": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "item": { "type": "string" },
                          "quantity": { "type": ["integer", "null"] },
                          "unit_price": { "type": ["number", "null"] },
                          "quality": { "type": "string", "enum": ["regular", "special"] }
                        },
                        "required": ["item", "quantity", "unit_price", "quality"],
                        "additionalProperties": false
                      }
                    },
                    "expenses": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "description": { "type": "string" },
                          "amount": { "type": "number" }
                        },
                        "required": ["description", "amount"],
                        "additionalProperties": false
                      }
                    }
                  },
                  "required": ["total_sale_price", "payment_method", "sales", "expenses"],
                  "additionalProperties": false
                }
                """),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await _openAiClient.CompleteChatAsync(messages, options);

        JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);
        return structuredJson;
    }
}