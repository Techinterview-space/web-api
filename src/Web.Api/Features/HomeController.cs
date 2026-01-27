using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Web.Api.Features;

// TO be removed
[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("configs")]
    public IActionResult GetConfigs()
    {
        var configs = GetSectionValues(_configuration.GetChildren());
        return Ok(configs);
    }

    private IDictionary<string, object> GetSectionValues(
        IEnumerable<IConfigurationSection> sections)
    {
        var result = new Dictionary<string, object>();

        foreach (var section in sections)
        {
            // Check if the current section has children (is a complex section)
            var children = section.GetChildren().ToList();
            if (children.Any())
            {
                // If the section has children, recursively get their values
                result.Add(section.Key, GetSectionValues(children));
            }
            else
            {
                // If the section does not have children, it's a key-value pair
                result.Add(section.Key, section.Value);
            }
        }

        return result;
    }
}