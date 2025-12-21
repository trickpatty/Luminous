using FluentAssertions;
using Luminous.Api.Controllers;
using Luminous.Application.DTOs;
using Luminous.Application.Features.Families.Commands;
using Luminous.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Luminous.Api.Tests.Controllers;

public class FamiliesControllerTests
{
    private readonly Mock<ISender> _mediatorMock;
    private readonly FamiliesController _controller;

    public FamiliesControllerTests()
    {
        _mediatorMock = new Mock<ISender>();

        _controller = new FamiliesController();

        // Set up HttpContext with MediatR
        var httpContext = new DefaultHttpContext
        {
            RequestServices = new ServiceProviderMock(_mediatorMock.Object)
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Create_WithValidCommand_ShouldReturnCreatedResult()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Timezone = "America/New_York",
            OwnerEmail = "owner@example.com",
            OwnerDisplayName = "Test Owner"
        };

        var expectedFamily = new FamilyDto
        {
            Id = "family-123",
            Name = command.Name,
            Timezone = command.Timezone,
            MemberCount = 1
        };

        _mediatorMock.Setup(x => x.Send(It.IsAny<CreateFamilyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFamily);

        // Act
        var result = await _controller.Create(command);

        // Assert
        result.Result.Should().BeOfType<CreatedResult>();
        var createdResult = (CreatedResult)result.Result!;
        createdResult.Location.Should().Contain(expectedFamily.Id);

        var response = createdResult.Value as ApiResponse<FamilyDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data!.Name.Should().Be(command.Name);
    }

    private class ServiceProviderMock : IServiceProvider
    {
        private readonly ISender _sender;

        public ServiceProviderMock(ISender sender)
        {
            _sender = sender;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ISender))
                return _sender;
            return null;
        }
    }
}
