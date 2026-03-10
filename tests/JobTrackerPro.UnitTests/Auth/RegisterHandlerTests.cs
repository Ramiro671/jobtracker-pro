using FluentAssertions;
using JobTrackerPro.Application.Auth.Commands;
using JobTrackerPro.Application.Common.Interfaces;
using JobTrackerPro.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace JobTrackerPro.UnitTests.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _handler = new RegisterHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _jwtTokenGeneratorMock.Object,
            NullLogger<RegisterHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithNewEmail_ShouldReturnAuthResponse()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jwtTokenGeneratorMock
            .Setup(g => g.GenerateToken(It.IsAny<JobTrackerPro.Domain.Entities.User>()))
            .Returns("fake-jwt-token");

        var command = new RegisterCommand("Ramiro López", "ramiro@test.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("fake-jwt-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.ExistsAsync("duplicate@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new RegisterCommand("Test User", "duplicate@test.com", "Password123!");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*duplicate@test.com*");
    }
}
