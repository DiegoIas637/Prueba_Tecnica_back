namespace Application.DTOs;

/// <summary>Vista pública de un plan (R1).</summary>
public record PlanDto(
    string Code,
    string Name,
    decimal Premium,
    int AvailableQuota);
