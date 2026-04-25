using DeliveryService.Core.Entities;
using DeliveryService.Core.Enums;
using DeliveryService.Core.Interfaces;
using DeliveryService.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.API.Controllers;

[ApiController]
[Route("api/packages")]
public class PackagesController : ControllerBase
{
    private readonly IPackageRepository _packageRepository;
    private readonly ISenderRepository _senderRepository;
    private readonly ITrackingCodeGenerator _trackingCodeGenerator;

    public PackagesController(
        IPackageRepository packageRepository,
        ISenderRepository senderRepository,
        ITrackingCodeGenerator trackingCodeGenerator)
    {
        _packageRepository = packageRepository;
        _senderRepository = senderRepository;
        _trackingCodeGenerator = trackingCodeGenerator;
    }

    [HttpPost]
    public async Task<ActionResult<PackageResponseDto>> CreatePackage(CreatePackageDto dto)
    {
        if (dto.Weight <= 0 || dto.Weight > 50)
            return BadRequest("Weight must be between 0 and 50 kg.");

        var sender = await _senderRepository.GetByIdAsync(dto.SenderId);
        if (sender == null)
            return NotFound($"Sender with id {dto.SenderId} not found.");

        var trackingCode = _trackingCodeGenerator.Generate();
        while (await _packageRepository.ExistsByTrackingCodeAsync(trackingCode))
            trackingCode = _trackingCodeGenerator.Generate();

        var package = new Package
        {
            Id = Guid.NewGuid(),
            SenderId = dto.SenderId,
            RecipientName = dto.RecipientName,
            RecipientAddress = dto.RecipientAddress,
            RecipientPhone = dto.RecipientPhone,
            Weight = dto.Weight,
            Dimensions = dto.Dimensions,
            Status = PackageStatus.Created,
            TrackingCode = trackingCode,
            CreatedAt = DateTime.UtcNow
        };

        await _packageRepository.AddAsync(package);
        var update = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            Status = PackageStatus.Created,
            Location = "Warehouse",
            Notes = "Package created",
            Timestamp = DateTime.UtcNow,
            UpdatedBy = "system"
        };
        await _packageRepository.AddUpdateAsync(update);

        return Ok(MapToResponse(package));
    }

    [HttpGet("{trackingCode}")]
    public async Task<ActionResult<PackageResponseDto>> TrackPackage(string trackingCode)
    {
        var package = await _packageRepository.GetByTrackingCodeAsync(trackingCode);
        if (package == null)
            return NotFound($"Package with tracking code {trackingCode} not found.");
        return Ok(MapToResponse(package));
    }

    [HttpGet("{trackingCode}/updates")]
    public async Task<ActionResult<IEnumerable<DeliveryUpdateDto>>> GetUpdates(string trackingCode)
    {
        var package = await _packageRepository.GetByTrackingCodeAsync(trackingCode);
        if (package == null)
            return NotFound($"Package with tracking code {trackingCode} not found.");

        var updates = await _packageRepository.GetUpdatesForPackageAsync(package.Id);
        return Ok(updates.Select(u => new DeliveryUpdateDto
        {
            Status = u.Status.ToString(),
            Location = u.Location,
            Notes = u.Notes,
            Timestamp = u.Timestamp,
            UpdatedBy = u.UpdatedBy
        }));
    }

    [HttpPost("{id}/update")]
    public async Task<IActionResult> AddUpdate(Guid id, [FromBody] UpdateStatusDto dto)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        if (package == null)
            return NotFound($"Package with id {id} not found.");

        if (package.Status == PackageStatus.Delivered || package.Status == PackageStatus.Returned)
            return BadRequest("Cannot update delivered or returned package.");

        var newStatus = Enum.Parse<PackageStatus>(dto.Status);
        if (!IsValidTransition(package.Status, newStatus))
            return BadRequest($"Invalid status transition from {package.Status} to {newStatus}.");

        package.Status = newStatus;
        var update = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            Status = newStatus,
            Location = dto.Location,
            Notes = dto.Notes,
            Timestamp = DateTime.UtcNow,
            UpdatedBy = dto.UpdatedBy
        };
        await _packageRepository.UpdatePackageStatusAsync(package, update);
        return Ok();
    }

    [HttpPatch("{id}/deliver")]
    public async Task<IActionResult> MarkAsDelivered(Guid id)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        if (package == null) return NotFound();
        if (package.Status != PackageStatus.OutForDelivery)
            return BadRequest("Package must be OutForDelivery before delivering.");

        package.Status = PackageStatus.Delivered;
        var update = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            Status = PackageStatus.Delivered,
            Location = package.RecipientAddress,
            Notes = "Delivered to recipient",
            Timestamp = DateTime.UtcNow,
            UpdatedBy = "system"
        };
        await _packageRepository.UpdatePackageStatusAsync(package, update);
        return Ok();
    }

    [HttpPatch("{id}/return")]
    public async Task<IActionResult> MarkAsReturned(Guid id)
    {
        var package = await _packageRepository.GetByIdAsync(id);
        if (package == null) return NotFound();
        if (package.Status == PackageStatus.Delivered)
            return BadRequest("Cannot return a delivered package.");

        package.Status = PackageStatus.Returned;
        var update = new DeliveryUpdate
        {
            Id = Guid.NewGuid(),
            PackageId = package.Id,
            Status = PackageStatus.Returned,
            Location = "Sender address",
            Notes = "Returned to sender",
            Timestamp = DateTime.UtcNow,
            UpdatedBy = "system"
        };
        await _packageRepository.UpdatePackageStatusAsync(package, update);
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackageResponseDto>>> GetByStatus([FromQuery] string? status)
    {
        var packages = string.IsNullOrEmpty(status)
            ? await _packageRepository.GetAllAsync()
            : await _packageRepository.GetByStatusAsync(Enum.Parse<PackageStatus>(status));
        return Ok(packages.Select(MapToResponse));
    }

    private bool IsValidTransition(PackageStatus current, PackageStatus next)
    {
        return next switch
        {
            PackageStatus.PickedUp => current == PackageStatus.Created,
            PackageStatus.InTransit => current == PackageStatus.PickedUp,
            PackageStatus.OutForDelivery => current == PackageStatus.InTransit,
            PackageStatus.Delivered => current == PackageStatus.OutForDelivery,
            PackageStatus.Returned => current != PackageStatus.Delivered,
            _ => false
        };
    }

    private PackageResponseDto MapToResponse(Package p) => new()
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
    };
}