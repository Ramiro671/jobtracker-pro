# Day 13 — Unit Tests: xUnit + Moq + FluentAssertions

**Date:** March 10, 2026
**Phase:** 1 — Backend Core
**Block:** Bloque 16 — Unit Tests: xUnit + Moq + FluentAssertions
**Duration:** ~1 hour

---

## What I did

Wrote 8 unit tests covering the Application layer handlers.
All tests pass with zero failures. No database or network required.

---

## Test results

```
total: 11 · failed: 0 · succeeded: 11 · skipped: 0
duration: 7.4s
```

---

## Files created

```
tests/JobTrackerPro.UnitTests/
├── JobApplications/
│   ├── CreateJobApplicationHandlerTests.cs   (3 tests)
│   └── GetJobApplicationsHandlerTests.cs     (2 tests)
└── Auth/
    └── LoginHandlerTests.cs                  (3 tests)
```

---

## NuGet packages added

```bash
dotnet add tests/JobTrackerPro.UnitTests package Moq
dotnet add tests/JobTrackerPro.UnitTests package FluentAssertions
dotnet add tests/JobTrackerPro.UnitTests package Microsoft.Extensions.Logging.Abstractions
```

---

## Tests written

### CreateJobApplicationHandlerTests (3 tests)

| Test | What it verifies |
|------|-----------------|
| `Handle_WhenCompanyDoesNotExist_ShouldCreateCompanyAndApplication` | New company created + application saved + SaveChanges called once |
| `Handle_WhenCompanyAlreadyExists_ShouldReuseCompany` | Existing company reused — AddAsync NOT called again |
| `Handle_ShouldReturnValidGuid` | Returns non-empty Guid |

### GetJobApplicationsHandlerTests (2 tests)

| Test | What it verifies |
|------|-----------------|
| `Handle_WhenUserHasApplications_ShouldReturnMappedDtos` | Returns correct count, all DTOs have correct UserId |
| `Handle_WhenUserHasNoApplications_ShouldReturnEmptyList` | Returns empty list (not null) |

### LoginHandlerTests (3 tests)

| Test | What it verifies |
|------|-----------------|
| `Handle_WithValidCredentials_ShouldReturnAuthResponse` | Returns AuthResponse with valid token + refreshToken |
| `Handle_WithWrongPassword_ShouldThrowUnauthorizedAccessException` | BCrypt mismatch → 401 |
| `Handle_WithNonExistentEmail_ShouldThrowUnauthorizedAccessException` | Unknown email → 401 |

---

## Pattern — Arrange · Act · Assert

```csharp
[Fact]
public async Task Handle_WhenCompanyDoesNotExist_ShouldCreateCompanyAndApplication()
{
    // Arrange — prepare mocks and inputs
    _companyRepositoryMock
        .Setup(r => r.GetByNameAsync(command.CompanyName, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Company?)null);

    // Act — execute the handler
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert — verify outcome
    result.Should().NotBeEmpty();
    _unitOfWorkMock.Verify(
        u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
        Times.Once);
}
```

---

## Key concepts

### Moq — fake dependencies without a database

```csharp
var mock = new Mock<IJobApplicationRepository>();

// Define what the fake returns
mock.Setup(r => r.GetByUserIdAsync(userId, ...))
    .ReturnsAsync(fakeList);

// Verify it was called exactly once
mock.Verify(r => r.AddAsync(...), Times.Once);
mock.Verify(r => r.AddAsync(...), Times.Never); // ← for negative assertions
```

### FluentAssertions — readable assertions

```csharp
result.Should().NotBeEmpty();
result.Should().HaveCount(2);
result.Should().AllSatisfy(dto => dto.UserId.Should().Be(userId));

await act.Should().ThrowAsync<UnauthorizedAccessException>()
    .WithMessage("Invalid email or password.");
```

### NullLogger — no need to mock ILogger

```csharp
NullLogger<CreateJobApplicationHandler>.Instance
// Accepts all log calls, does nothing — perfect for tests
```

---

## FluentAssertions license note

Free for non-commercial use (portfolio projects). For commercial use → Xceed subscription required. Alternative: `Shouldly` (fully free).

---

## Unit vs Integration tests

| Concern | Unit test | Integration test |
|---------|-----------|-----------------|
| Handler logic | ✅ | ❌ |
| HTTP routing | ❌ | ✅ |
| Database queries | ❌ | ✅ |
| Full pipeline | ❌ | ✅ |
| Speed | ⚡ milliseconds | 🐢 seconds |

---

## Phase 1 progress

| Block | Content | Status |
|-------|---------|--------|
| 1–15 | All previous blocks | ✅ |
| **16** | **Unit Tests: xUnit + Moq + FluentAssertions** | ✅ |
| 17 | Integration Tests: WebApplicationFactory | ⏳ next |

---

## Commit

```
test: add unit tests for application layer handlers
```
