using Microsoft.AspNetCore.Mvc;
using SmartBin.Application.Services;
using SmartBin.Domain.Models;
using SmartBin.Domain.Models.Dto;
using System.Security.Authentication;

namespace SmartBin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<ActionResult<List<User>>> Get()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Post([FromBody] User user)
    {
        var created = await _userService.CreateAsync(user);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] User user)
    {
        await _userService.UpdateAsync(id, user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registrationDto)
    {
        try
        {
            // Вызов метода из слоя Application
            var tokenPair = await _userService.RegisterAsync(registrationDto);

            // Возврат токенов клиенту
            return Ok(tokenPair);
        }
        catch (InvalidOperationException ex)
        {
            // Например, если пользователь уже существует
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto) // Создайте UserLoginDto, если нужно
    {
        try
        {
            // Вызов метода из слоя Application
            var tokenPair = await _userService.LoginAsync(loginDto.Nickname, loginDto.Password);

            // Возврат токенов клиенту
            return Ok(tokenPair);
        }
        catch (AuthenticationException ex)
        {
            // Неверный логин/пароль
            return Unauthorized(new { message = ex.Message });
        }
        // Здесь могут быть другие исключения, которые нужно обработать
    }
}