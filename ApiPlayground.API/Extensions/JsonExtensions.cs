using System.Text.Json;

namespace ApiPlayground.API.Extensions;

public static class JsonExtensions
{
    public static object? ExtractValueByJsonPath(this string jsonString, string jsonPath)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(jsonString);
            return ExtractFromElement(jsonDoc.RootElement, jsonPath);
        }
        catch
        {
            return null;
        }
    }
    
    private static object? ExtractFromElement(JsonElement element, string path)
    {
        if (string.IsNullOrEmpty(path) || path == "$")
            return GetElementValue(element);
            
        if (path.StartsWith("$."))
            path = path.Substring(2);
            
        var parts = path.Split('.', 2);
        var currentProperty = parts[0];
        var remainingPath = parts.Length > 1 ? parts[1] : "";
        
        // Handle array access like "data[0]"
        if (currentProperty.Contains('[') && currentProperty.Contains(']'))
        {
            var propertyName = currentProperty.Substring(0, currentProperty.IndexOf('['));
            var indexStr = currentProperty.Substring(currentProperty.IndexOf('[') + 1, 
                currentProperty.IndexOf(']') - currentProperty.IndexOf('[') - 1);
            
            if (int.TryParse(indexStr, out int index))
            {
                if (element.TryGetProperty(propertyName, out var arrayElement) && 
                    arrayElement.ValueKind == JsonValueKind.Array)
                {
                    var arrayEnum = arrayElement.EnumerateArray();
                    var items = arrayEnum.ToArray();
                    if (index >= 0 && index < items.Length)
                    {
                        return string.IsNullOrEmpty(remainingPath) 
                            ? GetElementValue(items[index])
                            : ExtractFromElement(items[index], remainingPath);
                    }
                }
            }
        }
        else if (element.TryGetProperty(currentProperty, out var property))
        {
            return string.IsNullOrEmpty(remainingPath) 
                ? GetElementValue(property)
                : ExtractFromElement(property, remainingPath);
        }
        
        return null;
    }
    
    private static object? GetElementValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => element.GetRawText(),
            JsonValueKind.Array => element.GetRawText(),
            _ => element.GetRawText()
        };
    }
}