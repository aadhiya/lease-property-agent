using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Services;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;

    public UnitService(IUnitRepository unitRepository)
    {
        _unitRepository = unitRepository;
    }

    public async Task<List<UnitListResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var units = await _unitRepository.GetAllAsync(
            cancellationToken);

        return units
            .Select(unit => new UnitListResponse
            {
                UnitId = unit.Id,
                UnitNumber = unit.UnitNumber,
                UnitType = unit.UnitType,
                Area = unit.Area,
                Status = unit.Status.ToString()
            })
            .ToList();
    }
}