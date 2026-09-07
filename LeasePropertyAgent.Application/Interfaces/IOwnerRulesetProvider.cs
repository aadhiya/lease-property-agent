using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IOwnerRulesetProvider
{
    Task<OwnerRuleset> GetRulesetAsync(
        CancellationToken cancellationToken = default);
}