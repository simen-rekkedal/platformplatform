using JetBrains.Annotations;
using PlatformPlatform.AccountManagement.Domain.Tenants;

namespace PlatformPlatform.AccountManagement.Application.Tenants;

[UsedImplicitly]
public sealed record TenantResponseDto(string Id, DateTime CreatedAt, DateTime? ModifiedAt, string Name,
    TenantState State, string? Phone);