using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PlatformPlatform.AccountManagement.Application.Tenants;
using PlatformPlatform.AccountManagement.Infrastructure;
using PlatformPlatform.SharedKernel.ApplicationCore.Validation;
using Xunit;

namespace PlatformPlatform.AccountManagement.Tests.Api.Tenants;

public sealed class TenantEndpointsTests : BaseApiTests<AccountManagementDbContext>
{
    [Fact]
    public async Task GetTenant_WhenTenantExists_ShouldReturnTenantWithValidContract()
    {
        // Act
        var response = await TestHttpClient.GetAsync($"/api/tenants/{DatabaseSeeder.Tenant1.Id}");

        // Assert
        EnsureSuccessGetRequest(response);

        var tenantDto = await response.Content.ReadFromJsonAsync<TenantResponseDto>();
        var createdAt = tenantDto?.CreatedAt.ToString(Iso8601TimeFormat);
        var expectedBody =
            $$"""{"id":"{{DatabaseSeeder.Tenant1.Id}}","createdAt":"{{createdAt}}","modifiedAt":null,"name":"{{DatabaseSeeder.Tenant1.Name}}","state":0,"phone":"1234567890"}""";
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Be(expectedBody);
    }

    [Fact]
    public async Task GetTenant_WhenTenantDoesNotExist_ShouldReturnNotFound()
    {
        // Act
        var response = await TestHttpClient.GetAsync("/api/tenants/unknown");

        // Assert
        await EnsureErrorStatusCode(response, HttpStatusCode.NotFound, "Tenant with id 'unknown' not found.");
    }

    [Fact]
    public async Task CreateTenant_WhenValid_ShouldCreateTenant()
    {
        // Act
        var command = new CreateTenant.Command("tenant2", "TestTenant", "1234567890", "test@test.com");
        var response = await TestHttpClient.PostAsJsonAsync("/api/tenants", command);

        // Assert
        await EnsureSuccessPostRequest(response, "/api/tenants/tenant2");
        Connection.ExecuteScalar(
                "SELECT COUNT(*) FROM Users WHERE TenantId = 'tenant2' AND UserRole = 'TenantOwner' AND Email = 'test@test.com'")
            .Should().Be(1);
    }

    [Fact]
    public async Task CreateTenant_WhenInvalid_ShouldReturnBadRequest()
    {
        // Act
        var command = new CreateTenant.Command("a", "TestTenant", null, "ab");
        var response = await TestHttpClient.PostAsJsonAsync("/api/tenants", command);

        // Assert
        var expectedErrors = new[]
        {
            new ErrorDetail("Email", "'Email' is not a valid email address."),
            new ErrorDetail("Subdomain", "Subdomain must be between 3-30 alphanumeric and lowercase characters.")
        };
        await EnsureErrorStatusCode(response, HttpStatusCode.BadRequest, expectedErrors);
    }

    [Fact]
    public async Task CreateTenant_WhenTenantExists_ShouldReturnBadRequest()
    {
        // Act
        var command = new CreateTenant.Command(DatabaseSeeder.Tenant1.Id, "TestTenant", null, "test@test.com");
        var response = await TestHttpClient.PostAsJsonAsync("/api/tenants", command);

        // Assert
        var expectedErrors = new[]
        {
            new ErrorDetail("Subdomain", "The subdomain is not available.")
        };
        await EnsureErrorStatusCode(response, HttpStatusCode.BadRequest, expectedErrors);
    }

    [Fact]
    public async Task UpdateTenant_WhenValid_ShouldUpdateTenant()
    {
        // Act
        var command = new UpdateTenant.Command {Name = "UpdatedName", Phone = "0987654321"};
        var response = await TestHttpClient.PutAsJsonAsync($"/api/tenants/{DatabaseSeeder.Tenant1.Id}", command);

        // Assert
        EnsureSuccessPutRequest(response);
    }

    [Fact]
    public async Task UpdateTenant_WhenInvalid_ShouldReturnBadRequest()
    {
        // Act
        var command = new UpdateTenant.Command {Name = "Invalid phone", Phone = "01-800-HOTLINE"};
        var response = await TestHttpClient.PutAsJsonAsync($"/api/tenants/{DatabaseSeeder.Tenant1.Id}", command);

        // Assert
        var expectedErrors = new[]
        {
            new ErrorDetail("Phone", "'Phone' is not in the correct format.")
        };
        await EnsureErrorStatusCode(response, HttpStatusCode.BadRequest, expectedErrors);
    }

    [Fact]
    public async Task UpdateTenant_WhenTenantDoesNotExists_ShouldReturnNotFound()
    {
        // Act
        var command = new UpdateTenant.Command {Name = "UpdatedName", Phone = "0987654321"};
        var response = await TestHttpClient.PutAsJsonAsync("/api/tenants/unknown", command);

        //Assert
        await EnsureErrorStatusCode(response, HttpStatusCode.NotFound, "Tenant with id 'unknown' not found.");
    }

    [Fact]
    public async Task DeleteTenant_WhenTenantDoesNotExists_ShouldReturnNotFound()
    {
        // Act
        var response = await TestHttpClient.DeleteAsync("/api/tenants/unknown");

        //Assert
        await EnsureErrorStatusCode(response, HttpStatusCode.NotFound, "Tenant with id 'unknown' not found.");
    }

    [Fact]
    public async Task DeleteTenant_WhenTenantWithUsersExists_ShouldReturnBadRequest()
    {
        // Act
        var response = await TestHttpClient.DeleteAsync($"/api/tenants/{DatabaseSeeder.Tenant1.Id}");

        // Assert
        var expectedErrors = new[]
        {
            new ErrorDetail("Id", "All users must be deleted before the tenant can be deleted.")
        };
        await EnsureErrorStatusCode(response, HttpStatusCode.BadRequest, expectedErrors);
    }

    [Fact]
    public async Task DeleteTenant_WhenTenantExistsWithNoUsers_ShouldDeleteTenant()
    {
        // Arrange
        var tenant1Id = DatabaseSeeder.Tenant1.Id;
        var _ = await TestHttpClient.DeleteAsync($"/api/users/{DatabaseSeeder.User1.Id}");

        // Act
        var response = await TestHttpClient.DeleteAsync($"/api/tenants/{tenant1Id}");

        // Assert
        EnsureSuccessDeleteRequest(response);

        // Verify that Tenant is deleted:
        Connection.ExecuteScalar("SELECT COUNT(*) FROM Tenants WHERE Id = @id", new {id = tenant1Id.ToString()})
            .Should().Be(0);
    }
}