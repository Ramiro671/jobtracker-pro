using FluentAssertions;
using JobTrackerPro.Application.JobApplications.Queries;
using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using Moq;

namespace JobTrackerPro.UnitTests.JobApplications;

public class GetJobApplicationsHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _repositoryMock;
    private readonly GetJobApplicationsHandler _handler;

    public GetJobApplicationsHandlerTests()
    {
        _repositoryMock = new Mock<IJobApplicationRepository>();
        _handler = new GetJobApplicationsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserHasApplications_ShouldReturnMappedDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var company = Company.Create("Google");

        var applications = new List<JobApplication>
        {
            JobApplication.Create(userId, "Senior Dev", company.Id, null, null, "LinkedIn"),
            JobApplication.Create(userId, "Backend Engineer", company.Id, null, null, "Direct")
        };

        _repositoryMock
            .Setup(r => r.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(applications);

        var query = new GetJobApplicationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Id.Should().NotBeEmpty());
    }

    [Fact]
    public async Task Handle_WhenUserHasNoApplications_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<JobApplication>());

        var query = new GetJobApplicationsQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
