using System.Text.Json;
using Ledon.BerryNet.Http;

namespace Ledon.BerryNet.Examples;

/// <summary>
/// HTTP客户端使用示例
/// </summary>
public class HttpClientExamples
{
    /// <summary>
    /// 基础GET请求示例
    /// </summary>
    public static async Task BasicGetExample()
    {
        var client = new BerryHttpClient();
        
        try
        {
            var response = await client
                .Get("https://jsonplaceholder.typicode.com/posts/1")
                .WithHeader("User-Agent", "BerryNet-Example")
                .ExecuteAsync();
            
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Content: {response.Content}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// POST请求示例
    /// </summary>
    public static async Task PostExample()
    {
        var client = new BerryHttpClient();
        
        try
        {
            var postData = new
            {
                title = "foo",
                body = "bar",
                userId = 1
            };

            var result = await client
                .Post("https://jsonplaceholder.typicode.com/posts")
                .WithJsonBody(postData)
                .ExecuteAsync<PostResponse>();
            
            Console.WriteLine($"Created post with ID: {result?.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }

    /// <summary>
    /// 工厂模式示例
    /// </summary>
    public static async Task FactoryExample()
    {
        var factory = new BerryHttpClientFactory();
        
        var client = factory.CreateClient();
        
        try
        {
            var users = await client
                .Get("https://jsonplaceholder.typicode.com/users")
                .WithQueryParameter("_limit", "5")
                .ExecuteAsync<List<User>>();
            
            Console.WriteLine($"Retrieved {users?.Count ?? 0} users");
            
            if (users != null)
            {
                foreach (var user in users)
                {
                    Console.WriteLine($"- {user.Name} ({user.Email})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 链式调用复杂示例
    /// </summary>
    public static async Task ComplexChainExample()
    {
        var client = new BerryHttpClient();
        
        try
        {
            var response = await client
                .Post("https://httpbin.org/post")
                .WithHeader("Authorization", "Bearer fake-token")
                .WithHeader("X-Custom-Header", "custom-value")
                .WithQueryParameter("test", "true")
                .WithQueryParameter("format", "json")
                .WithJsonBody(new { message = "Hello from BerryNet!" })
                .WithTimeout(TimeSpan.FromSeconds(30))
                .ExecuteAsync();
            
            if (response.IsSuccessful)
            {
                Console.WriteLine("Request successful!");
                Console.WriteLine($"Response: {response.Content}");
            }
            else
            {
                Console.WriteLine($"Request failed with status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}

/// <summary>
/// 示例数据模型
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public Address? Address { get; set; }
    public string Phone { get; set; } = "";
    public string Website { get; set; } = "";
    public Company? Company { get; set; }
}

public class Address
{
    public string Street { get; set; } = "";
    public string Suite { get; set; } = "";
    public string City { get; set; } = "";
    public string Zipcode { get; set; } = "";
    public Geo? Geo { get; set; }
}

public class Geo
{
    public string Lat { get; set; } = "";
    public string Lng { get; set; } = "";
}

public class Company
{
    public string Name { get; set; } = "";
    public string CatchPhrase { get; set; } = "";
    public string Bs { get; set; } = "";
}

public class PostResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public int UserId { get; set; }
}
