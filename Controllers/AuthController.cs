using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace github_auth_backend.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory clientFactory, ILogger<AuthController> logger)
    {
      _httpClientFactory = clientFactory;
      _logger = logger;
    }

    [HttpGet]
    [Route("CallBack")]
    public async Task<RedirectResult> Callback([FromQuery]string code)
    {
      var httpClient = _httpClientFactory.CreateClient();
      httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

      var clientId = "7b929a6152af142a8949";
      var clientSecret = "c389ce4c73a6fddf266a9fc31de9348413fa0c1f";

      var bodyContent = new StringContent(@$"", Encoding.UTF8, "application/json");
      var url = $"https://github.com/login/oauth/access_token?client_id={clientId}&client_secret={clientSecret}&code={code}";
      _logger.LogInformation(url);
      var responseToken = await httpClient.PostAsync(
        url,
        null
      );
      var str = await responseToken.Content.ReadAsStringAsync();
      var token = JsonSerializer.Deserialize<TokenStruct>(str);
      _logger.LogInformation(token.access_token);
      _logger.LogInformation(token.scope);
      _logger.LogInformation(token.token_type);

      var response = new HttpResponseMessage();
      var cookieOptions = new CookieOptions();
      cookieOptions.Expires = DateTime.Now.AddMinutes(10);  

      Response.Cookies.Append("token", token.access_token, cookieOptions);  
      return base.Redirect("http://localhost:3000");
    }

    [HttpGet]
    [Route("GetUserInfo")]
    public async Task<UserInfoStruct> GetUserInfo(string token)
    {
      var httpClient = _httpClientFactory.CreateClient();
      httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
      httpClient.DefaultRequestHeaders.Add("user-agent", "request");

      var url = $"https://api.github.com/user?access_token={token}";
      _logger.LogInformation(url);
      var userInfoResponse = await httpClient.GetAsync(url);
      var str = await userInfoResponse.Content.ReadAsStringAsync();
      _logger.LogInformation(str);
      var userInfo = JsonSerializer.Deserialize<UserInfoStruct>(str);
      return userInfo;
    }

    public class TokenStruct
    {
      public string access_token { get; set; }
      public string token_type { get; set; }
      public string scope { get; set; }
    }

    public class UserInfoStruct
    {
      public string email { get; set; }
      public string login { get; set; }
      public string name { get; set; }
    }
  }
}