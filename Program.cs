using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    // Create a user on current thread from provided header
    if (context.Request.Headers.ContainsKey("X-MS-CLIENT-PRINCIPAL-ID"))
    {
        // Read headers from Azure
        var azureAppServicePrincipalIdHeader = context.Request.Headers["X-MS-CLIENT-PRINCIPAL-ID"][0];

        #region extract claims via call /.auth/me
        //invoke /.auth/me
        var cookieContainer = new CookieContainer();
        HttpClientHandler handler = new HttpClientHandler()
        {
            CookieContainer = cookieContainer
        };
        string uriString = $"{context.Request.Scheme}://{context.Request.Host}";
        // pass the cookies into the new handler
        foreach (var c in context.Request.Cookies)
        {
            cookieContainer.Add(new Uri(uriString), new Cookie(c.Key, c.Value));
        }
        string jsonResult;
        using (HttpClient client = new HttpClient(handler))
        {
            var res = await client.GetAsync($"{uriString}/.auth/me");
            jsonResult = await res.Content.ReadAsStringAsync();
        }

        //parse json
        var obj = JArray.Parse(jsonResult);

        // Create claims id
        List<Claim> claims = new List<Claim>();
        foreach (var claim in obj[0]["user_claims"])
        {
            claims.Add(new Claim(claim["typ"]?.ToString(), claim["val"]?.ToString()));
        }

        // Set user in current context as claims principal
        var identity = new GenericIdentity(azureAppServicePrincipalIdHeader);
        identity.AddClaims(claims);
        #endregion

        // Set current thread user to identity
        context.User = new GenericPrincipal(identity, null);
    };

    await next.Invoke();
});


app.MapRazorPages();

app.Run();
