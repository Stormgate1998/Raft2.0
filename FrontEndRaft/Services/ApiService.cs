namespace FrontEndRaft.Services;

public class ApiService
{
    HttpClient httpClient;
    public ApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private async Task AddToLog(string key, string value)
    {
        await httpClient.PostAsync("Gateway/AddToLog", new LogObject(key, value));
    }

    private async Task CompareVersionAndSwap(string key, string newValue, string expectedVersion)
    {
        await httpClient.PostAsync($"Gateway/CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);
        string result = await response.Content.ReadAsStringAsync();

        return result;
    }

    private async Task<(string, int)> EventualGet(string key)
    {
        var response = await httpClient.GetAsync($"Gateway/EventualGet/{key}");
        string result = await response.Content.ReadAsStringAsync();
        return ParseString(result);
    }
    private async Task<(string, int)> StrongGet(string key)
    {
        var response = await httpClient.GetAsync($"Gateway/StrongGet/{key}");
        string result = await response.Content.ReadAsStringAsync();
        return ParseString(result);
    }


    private static (string, int) ParseString(string input)
    {
        string[] parts = input.Split(',');
        if (parts.Length != 2)
        {
            throw new FormatException("Input string is not in the correct format.");
        }

        string str = parts[0];
        if (!int.TryParse(parts[1], out int intValue))
        {
            throw new FormatException("Integer part of the input string is not a valid integer.");
        }

        return (str, intValue);
    }
}
public class LogObject(string key, string value)
{
    public string key = key;
    public string value = value;

}
