namespace MesaSitec.Controllers;

using Microsoft.AspNetCore.Mvc;
using MesaSitec.Services;
using MesaSitec.Dtos;
using Microsoft.OpenApi.Expressions;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController (AuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Email) ||string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { codigo = "VALIDACION", detail = "Email y password requeridos"});

        var response = _authService.Login (request.Email, request.Password);

        if(response == null)
            return Unauthorized(new {codigo = "No_Auntentico", detail = "Email o contraseña incorrectos"});

            return Ok(response);
    }       
}