﻿using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using spa.server.Model;

namespace spa.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private IConfiguration _configration;

    public ConfigurationController(IConfiguration configuration)
    {
        _configration = configuration;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var jsConfig = new StringBuilder();
        jsConfig.Append("window.env = {");
        foreach (var item in _configration.GetSection("spa").GetChildren())
        {
            jsConfig.Append(item.Key);
            jsConfig.Append(": ");
            jsConfig.Append($"\"{item.Value}\",");
        }

        jsConfig.Append("}");
        return new JavaScriptResult(jsConfig.ToString());
    }
}