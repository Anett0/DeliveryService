using DeliveryService.Core.Interfaces;
using DeliveryService.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers;

[ApiController]
[Route("api/senders")]
public class SendersController : ControllerBase
{
    private readonly ISenderRepository _senderRepository;

    public SendersController(ISenderRepository senderRepository)
    {
        _senderRepository = senderRepository;
    }

    [HttpGet("{id}/packages")]
    public async Task<ActionResult<IEnumerable<PackageResponseDto>>> GetSenderPackages(Guid id)
    {
        var sender = await _senderRepository.GetByIdAsync(id);
        if (sender == null)
            return NotFound($"Sender with id {id} not found.");

        var packages = await _senderRepository.GetPackagesBySenderIdAsync(id);
        return Ok(packages.Select(p => new PackageResponseDto
        {
            Id = p.Id,
            SenderId = p.SenderId,
            RecipientName = p.RecipientName,
            RecipientAddress = p.RecipientAddress,
            RecipientPhone = p.RecipientPhone,
            Weight = p.Weight,
            Dimensions = p.Dimensions,
            Status = p.Status.ToString(),
            TrackingCode = p.TrackingCode,
            CreatedAt = p.CreatedAt
        }));
    }
}