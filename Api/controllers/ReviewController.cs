using Api.Models;
using Api.services;
using Microsoft.AspNetCore.Mvc;

namespace Api.controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly CodeReviewService _reviewservice;

    public ReviewController(CodeReviewService reviewservice)
    {
        _reviewservice = reviewservice;
    }

    [HttpPost]
    public IActionResult Review(CodeReviewRequest request)
    {
        return Ok(new CodeReviewResponse
        {
            Review = _reviewservice.ReviewCode(request.Code)
        });

    }
}
