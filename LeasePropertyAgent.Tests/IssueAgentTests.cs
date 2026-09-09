using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Infrastructure.IssueAgents;
using Xunit;

namespace LeasePropertyAgent.Tests;

public class IssueAgentTests
{
    [Fact]
    public async Task AssessAsync_ShouldProduceIssueAssessment()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = new List<IssueImageInput>
        {
            new()
            {
                FileName = "kitchen.jpg",
                FilePath = "test-data/kitchen.jpg"
            }
        };

        // Act
        var result = await agent.AssessAsync(images);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Assessment);
        Assert.False(string.IsNullOrWhiteSpace(result.Assessment.Title));
        Assert.False(string.IsNullOrWhiteSpace(result.Assessment.Description));
        Assert.False(string.IsNullOrWhiteSpace(
            result.Assessment.ConditionAssessment));
    }

    [Fact]
    public async Task AssessAsync_ShouldRetainImageLevelEvidence()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = new List<IssueImageInput>
        {
            new()
            {
                FileName = "kitchen.jpg",
                FilePath = "test-data/kitchen.jpg"
            },
            new()
            {
                FileName = "sink.jpg",
                FilePath = "test-data/sink.jpg"
            }
        };

        // Act
        var result = await agent.AssessAsync(images);

        // Assert
        Assert.Equal(2, result.Assessment.ImageAssessments.Count);

        Assert.Contains(
            result.Assessment.ImageAssessments,
            x => x.FileName == "kitchen.jpg");

        Assert.Contains(
            result.Assessment.ImageAssessments,
            x => x.FileName == "sink.jpg");
    }

    [Fact]
    public async Task AssessAsync_ShouldIdentifyVisibleItems()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = new List<IssueImageInput>
        {
            new()
            {
                FileName = "kitchen.jpg",
                FilePath = "test-data/kitchen.jpg"
            }
        };

        // Act
        var result = await agent.AssessAsync(images);

        // Assert
        var assessment = result.Assessment.ImageAssessments.Single();

        Assert.NotEmpty(assessment.VisibleItems);
    }

    [Fact]
    public async Task AssessAsync_ShouldProduceWorkOrderDraft()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = new List<IssueImageInput>
        {
            new()
            {
                FileName = "kitchen.jpg",
                FilePath = "test-data/kitchen.jpg"
            }
        };

        // Act
        var result = await agent.AssessAsync(images);

        // Assert
        Assert.NotNull(result.WorkOrder);
        Assert.False(string.IsNullOrWhiteSpace(result.WorkOrder.Title));
        Assert.False(string.IsNullOrWhiteSpace(
            result.WorkOrder.Description));
    }

    [Fact]
    public async Task AssessAsync_ShouldReturnConfidenceWithinValidRange()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = new List<IssueImageInput>
        {
            new()
            {
                FileName = "kitchen.jpg",
                FilePath = "test-data/kitchen.jpg"
            }
        };

        // Act
        var result = await agent.AssessAsync(images);

        // Assert
        Assert.InRange(result.Assessment.Confidence, 0m, 1m);

        foreach (var imageAssessment
                 in result.Assessment.ImageAssessments)
        {
            Assert.InRange(imageAssessment.Confidence, 0m, 1m);
        }
    }

    [Fact]
    public async Task AssessAsync_ShouldRejectEmptyImageCollection()
    {
        // Arrange
        var agent = new StubIssueAgent();

        var images = Array.Empty<IssueImageInput>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => agent.AssessAsync(images));
    }
}