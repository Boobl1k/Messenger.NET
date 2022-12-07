using Microsoft.AspNetCore.Mvc;

namespace MyBook.Controllers;

public class AboutController : Controller
{
    // GET
    public IActionResult Index() => 
        View();
    
    public IActionResult Payments() => 
        View();
}