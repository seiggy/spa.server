using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using spa.server.Model;

namespace spa.server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IVariantFeatureManagerSnapshot _featureManagerSnapshot;
    private readonly IVariantFeatureManager _featureManager;

    public ConfigurationController(
        IConfiguration configuration, 
        IVariantFeatureManagerSnapshot featureManagerSnapshot,
        IVariantFeatureManager featureManager)
    {
        _configuration = configuration;
        _featureManagerSnapshot = featureManagerSnapshot;
        _featureManager = featureManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var jsConfig = new StringBuilder();
        jsConfig.Append("window.env = {");
        foreach (var item in _configuration.GetSection("spa").GetChildren())
        {
            jsConfig.Append($"{item.Key}: \"{item.Value}\",");
        }

        jsConfig.Append("features: {");
        var first = true;
        // Retrieve feature flags and their variants
        await foreach (var feature in _featureManager.GetFeatureNamesAsync(HttpContext.RequestAborted))
        {
            if (!first)
            {
                jsConfig.Append(',');
            }
            else
            {
                first = false;
            }

            var isEnabled = await _featureManager.IsEnabledAsync(feature);
            jsConfig.Append($"{feature}: {{ enabled: {isEnabled.ToString().ToLower()}");

            // Check for feature variants
            var featureDefinition = _configuration.GetSection($"FeatureManagement:{feature}").Get<FeatureDefinition>();

            if (featureDefinition?.EnabledFor != null)
            {
                jsConfig.Append(", variants: [");
                foreach (var variant in featureDefinition.EnabledFor)
                {
                    jsConfig.Append($"{{ name: \"{variant.Name}\", parameters: {JsonSerializer.Serialize(variant.Parameters)} }},");
                }
                jsConfig.Append(']');
            }
            var assignedVariant = await _featureManager.GetVariantAsync(feature, HttpContext.RequestAborted);
            if (assignedVariant != null)
            {
                jsConfig.Append($", activeVariant: {{ name: \"{assignedVariant.Name}\", value: \"{assignedVariant.Configuration.Value}\" }}");
            }
            jsConfig.Append('}');
        }
        jsConfig.Append('}');

        jsConfig.Append('}');
        return new JavaScriptResult(jsConfig.ToString());
    }
}