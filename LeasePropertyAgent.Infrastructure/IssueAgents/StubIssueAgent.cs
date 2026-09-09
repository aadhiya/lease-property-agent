using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Infrastructure.IssueAgents;

/// <summary>
/// Provides deterministic issue assessments for local development and tests.
/// The implementation intentionally keeps the AI boundary behind IIssueAgent
/// so a real vision model can replace it without changing the application flow.
/// </summary>
public class StubIssueAgent : IIssueAgent
{
    public Task<IssueAgentResult> AssessAsync(
        IReadOnlyList<IssueImageInput> images,
        CancellationToken cancellationToken = default)
    {
        if (images is null || images.Count == 0)
        {
            throw new ArgumentException(
                "At least one issue image is required.",
                nameof(images));
        }

        var imageAssessments = images
            .Select(image => new IssueImageAssessment
            {
                FileName = image.FileName,
                Observation =
                    "Visible signs of wear or damage are present in the inspected area.",
                Confidence = 0.90m,
                VisibleItems = new List<string>
                {
                    "Visible fixture",
                    "Adjacent surface"
                }
            })
            .ToList();

        var result = new IssueAgentResult
        {
            Assessment = new IssueAssessment
            {
                Title = "Property condition issue",
                Description =
                    "The supplied property photos show a visible condition requiring review.",
                ConditionAssessment =
                    "Visible wear or damage should be inspected and assessed for repair.",
                Confidence = 0.90m,
                ImageAssessments = imageAssessments
            },

            WorkOrder = new WorkOrderDraft
            {
                Title = "Inspect reported property condition",
                Description =
                    "Inspect the reported condition shown in the supplied photos and determine the appropriate repair or maintenance action."
            }
        };

        return Task.FromResult(result);
    }
}