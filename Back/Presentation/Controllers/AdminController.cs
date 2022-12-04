using Microsoft.AspNetCore.Mvc;
using Presentation.RabbitMQ.Producer;
using Presentation.Services;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService) =>
        _adminService = adminService;

    public record FreeAdminData(string AdminName);
    
    [HttpPost("free")]
    public IActionResult FreeAdmin([FromBody] FreeAdminData adminData)
    {
        _adminService.FreeAdmin(adminData.AdminName);
        return Ok();
    }

    [HttpGet("find")]
    public async Task<IActionResult> GetAdminForUser(string userName)
    {
        return Ok(new { adminName = await _adminService.GetAdmin(userName) });
    }
}