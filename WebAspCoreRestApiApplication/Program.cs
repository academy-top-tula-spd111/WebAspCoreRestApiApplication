using System.Text.RegularExpressions;
using WebAspCoreRestApiApplication;

List<User> users = new List<User>
{
    new User(){ Name="Bob", Age = 23, Id = Guid.NewGuid().ToString() },
    new User(){ Name="Jim", Age = 34, Id = Guid.NewGuid().ToString() },
    new User(){ Name="Tom", Age = 41, Id = Guid.NewGuid().ToString() }
};
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    var request = context.Request;
    var response = context.Response;
    var path = request.Path;

    string regexpGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path == "/api/users" && request.Method == "GET")
    {
        await GetAllUsers(response);
    }
    else if(Regex.IsMatch(path, regexpGuid) && request.Method == "GET")
    {
        string id = path.Value.Split("/")[3];
        await GetUser(id, response);
    }
    else if(path == "/api/users" && request.Method == "POST")
    {
        await CreateUser(response, request);
    }
    else if(path == "/api/users" && request.Method == "PUT")
    {
        await UpdateUser(response, request);
    }
    else if(Regex.IsMatch(path, regexpGuid) && request.Method == "DELETE")
    {
        string id = path.Value.Split("/")[3];
        await DeleteUser(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset = utf-8";
        await response.SendFileAsync("html/index.html");
    }

});

app.Run();




async Task GetAllUsers(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

async Task GetUser(string? id, HttpResponse response)
{
    User? user = users.FirstOrDefault(u => u.Id == id);
    if(user != null)
        await response.WriteAsJsonAsync(user);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "User not found" });
    }    
}

async Task CreateUser(HttpResponse response, HttpRequest request)
{
    try
    {
        var user = await request.ReadFromJsonAsync<User>();
        if (user != null)
        {
            user.Id = Guid.NewGuid().ToString();
            users.Add(user);
            await response.WriteAsJsonAsync(user);
        }
        else
            throw new Exception("Incorrect data");
    }
    catch(Exception ex)
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Incorrect data" });
    }
}

async Task UpdateUser(HttpResponse response, HttpRequest request)
{
    try
    {
        User? userData = await request.ReadFromJsonAsync<User>();
        if(userData != null)
        {
            var user = users.FirstOrDefault(u => u.Id == userData.Id);
            if(user != null)
            {
                user.Name = userData.Name;
                user.Age = userData.Age;
                Console.WriteLine(user.Id);
                await response.WriteAsJsonAsync(user);
            }
        }
        else
        {
            response.StatusCode = 404;
            await response.WriteAsJsonAsync(new { message = "User not found" });
        }
    }
    catch(Exception ex)
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Incorrect data" });
    }
}

async Task DeleteUser(string? id, HttpResponse response)
{
    User user = users.FirstOrDefault(u => u.Id == id);
    if(user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "User not found" });
    }
}