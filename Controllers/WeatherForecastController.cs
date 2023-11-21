using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gnyang.samples.encrytion.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ICryptoService _cryptoService;
    private readonly IHashService _hashService;

    public WeatherForecastController(ICryptoService cryptoService, IHashService hashService)
    {
        _cryptoService = cryptoService;
        _hashService = hashService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost("CryptoTest")]
    public async Task<IActionResult> CryptoTest([FromBody] LoginDto loginDto)
    {
        string salt = "salt_FindByAccount";
        string pass = "pass_FindByAccount";

        var encrypted = await _cryptoService.EncryptAsync(loginDto.Password, salt, pass);
        var decrypted = await _cryptoService.DecryptAsync(encrypted, salt, pass);

        return Ok(new ResponseDto { Result = "Success", Message = decrypted });
    }

    [HttpPost("HastTest")]
    public IActionResult HashTest([FromBody] LoginDto loginDto)
    {
        var hash = _hashService.Hashing(loginDto.Password, out var salt);

        AccountDto accountDto = new AccountDto
        {
            Account = loginDto.Account,
            Salt = Convert.ToHexString(salt)!,
            Hash = hash.ToString(),
        };

        var verified = _hashService.VerifyPassword(loginDto.Password,
            accountDto.Hash,
            Convert.FromHexString(accountDto.Salt));

        return Ok(new ResponseDto { Result = "Success", Message = verified.ToString() });
    }

}
