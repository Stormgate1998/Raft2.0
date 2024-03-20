using System.Collections;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
namespace RaftFrontEnd.Services;

public class ApiService
{
    private readonly HttpClient httpClient;
    public ApiService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private async Task AddToLog(string key, string value)
    {
        await httpClient.PostAsync($"Gateway/AddToLog/{key}/{value}", null);
    }

    private async Task CompareVersionAndSwap(string key, string newValue, string expectedVersion)
    {
        Console.WriteLine($"CompareVersionAndSwap: {key},{newValue}");
        await httpClient.PostAsync($"Gateway/CompareVersionAndSwap/{key}/{newValue}/{expectedVersion}", null);
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
        Console.WriteLine(result);
        return ParseString(result);
    }

    private static (string, int) ParseString(string input)
    {
        if (input.Length > 0)
        {

            string[] parts = input.Split(',');
            if (parts.Length != 2)
            {
                Console.WriteLine($"Input string is not in the correct format.: {input}");
            }

            string str = parts[0];
            if (!int.TryParse(parts[1], out int intValue))
            {
                Console.WriteLine($"Integer part of the input string is not a valid integer.:{input}");
            }
            if (str == "0")
            {
                str = "0";
            }

            return (str, intValue);
        }
        return ("0", 0);
    }

    public async Task<(string, int)> StrongGetBalance(string key)
    {
        string newKey = "balance-of_"+key;
        string value;
        int number;
        (value, number) = await StrongGet( newKey);
        if (number == 0)
        {
            await AddToLog( newKey, "0");
            (value, number) = await StrongGet( newKey);
        }
        return (value, number);
    }
    


    public async Task AddBalance(string username)
    {
        string value;
        int number;
        (value, number) = await StrongGet($"balance-of_{username}");
        if (number == 0)
        {
            Console.WriteLine($"Adding log balance-of {username} to log");
            await AddToLog($"balance-of_{username}", "0");
        }
    }
    public async Task ModifyBalance(string username, int balancechange)
    {
        string key = $"balance-of_{username}";
        var (value, number) = await StrongGet(key);
        int result;
        if (value == "0")
        {
            result = 0;
        }
        else
        {
            result = int.Parse(value);
        }
        Console.WriteLine($"result before change : {result}");
        int newResult = result + balancechange;
        Console.WriteLine($"result after change : {newResult}");

        await CompareVersionAndSwap(key, newResult.ToString(), number.ToString());


    }


     public async Task<(string, int)> StrongGetStock(string key)
    {
        string newKey = "stock-of_"+key;
        string value;
        int number;
        (value, number) = await StrongGet( newKey);
        if (number == 0)
        {
            await AddToLog( newKey, "0");
            (value, number) = await StrongGet( newKey);
        }
        return (value, number);
    }
    


    public async Task AddStock(string username)
    {
        string value;
        int number;
        (value, number) = await StrongGet($"stock-of_{username}");
        if (number == 0)
        {
            Console.WriteLine($"Adding log stock-of {username} to log");
            await AddToLog($"stock-of_{username}", "0");
        }
    }
    public async Task ModifyStock(string username, int balancechange)
    {
        string key = $"stock-of_{username}";
        var (value, number) = await StrongGet(key);
        int result;
        if (value == "0")
        {
            result = 0;
        }
        else
        {
            result = int.Parse(value);
        }
        Console.WriteLine($"result before change : {result}");
        int newResult = result + balancechange;
        Console.WriteLine($"result after change : {newResult}");

        await CompareVersionAndSwap(key, newResult.ToString(), number.ToString());


    }
}
public class LogObject(string key, string value)
{
    public string key = key;
    public string value = value;

}
