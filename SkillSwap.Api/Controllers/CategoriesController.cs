using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Features.Categories;

namespace SkillSwap.Api.Controllers;

public class CategoriesController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetCategories()
    {
        var result = await Mediator.Send(new GetCategoriesQuery());
        return HandleResult(result);
    }
}
