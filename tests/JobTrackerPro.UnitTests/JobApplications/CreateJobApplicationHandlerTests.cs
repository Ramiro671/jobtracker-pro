using FluentAssertions;
using JobTrackerPro.Application.JobApplications.Commands;
using JobTrackerPro.Domain.Entities;
using JobTrackerPro.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace JobTrackerPro.UnitTests.JobApplications;

public class CreateJobApplicationHandlerTests
{
    private readonly Mock<IJobApplicationRepository> _jobApplicationRepositoryMock;
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateJobApplicationHandler _handler;

    public CreateJobApplicationHandlerTests()
    {
        _jobApplicationRepositoryMock = new Mock<IJobApplicationRepository>();
        _companyRepositoryMock = new Mock<ICompanyRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateJobApplicationHandler(
            _jobApplicationRepositoryMock.Object,
            _companyRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<CreateJobApplicationHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WhenCompanyDoesNotExist_ShouldCreateCompanyAndApplication()
    {
        // Arrange
        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "Senior .NET Developer",
            CompanyName: "Anthropic",
            JobUrl: "https://anthropic.com/jobs/123",
            Description: "Remote position",
            Source: "LinkedIn");

        _companyRepositoryMock
            .Setup(r => r.GetByNameAsync(command.CompanyName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _companyRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _jobApplicationRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCompanyAlreadyExists_ShouldReuseCompany()
    {
        // Arrange
        var existingCompany = Company.Create("Google");

        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "Backend Engineer",
            CompanyName: "Google",
            JobUrl: "https://careers.google.com/1",
            Description: null,
            Source: "LinkedIn");

        _companyRepositoryMock
            .Setup(r => r.GetByNameAsync("Google", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCompany);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        _companyRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Company>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidGuid()
    {
        // Arrange
        var command = new CreateJobApplicationCommand(
            UserId: Guid.NewGuid(),
            Title: "Developer",
            CompanyName: "Microsoft",
            JobUrl: null,
            Description: null,
            Source: "Direct");

        _companyRepositoryMock
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Company?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
    }
}
