using Application.DTOs;
using Domain.Entities;

namespace Application.Mapping;

internal static class SaleMappings
{
    public static SaleDto ToDto(this Sale sale) => new(
        sale.Id,
        sale.RequestReference,
        sale.AdvisorId,
        sale.PlanCode,
        sale.PlanName,
        sale.BuyerName,
        sale.BuyerEmail,
        sale.Premium,
        sale.AcceptedAt);

    public static SaleListItemDto ToListItemDto(this Sale sale) => new(
        sale.Id,
        sale.RequestReference,
        sale.PlanName,
        sale.Premium,
        sale.BuyerName,
        sale.AcceptedAt);

    public static PlanDto ToDto(this Plan plan) => new(
        plan.Code,
        plan.Name,
        plan.Premium,
        plan.AvailableQuota);
}
