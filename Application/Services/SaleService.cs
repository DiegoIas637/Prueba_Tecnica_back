using Application.Abstractions;
using Application.DTOs;
// ConcurrencyConflictException vive en Application.Abstractions (ya importado arriba).
using Application.Mapping;
using Application.Validation;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Repositories;

namespace Application.Services;

/// <summary>
/// Caso de uso central. Implementa el registro de ventas con las garantías R3:
/// idempotencia por (asesor, referencia), rechazo de cambios sobre referencia
/// aceptada, serialización de reintentos concurrentes y cupos nunca negativos.
/// </summary>
public sealed class SaleService : ISaleService
{
    private const int MaxPageSize = 20;
    private const int MaxConcurrencyRetries = 5;

    private readonly ISaleRepository _saleRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISaleRegistrationLock _registrationLock;

    public SaleService(
        ISaleRepository saleRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork,
        ISaleRegistrationLock registrationLock)
    {
        _saleRepository = saleRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
        _registrationLock = registrationLock;
    }

    public async Task<SaleDto> RegisterSaleAsync(
        string advisorId,
        RegisterSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(advisorId))
            throw new InvalidSaleDataException("El asesor activo es obligatorio.");

        // Validación de entrada (nombre, email y referencia).
        ReferenceValidator.Validate(request.RequestReference);
        BuyerValidator.Validate(request.BuyerName, request.BuyerEmail);

        var reference = request.RequestReference.Trim();
        var buyerName = request.BuyerName.Trim();
        var buyerEmail = request.BuyerEmail.Trim();
        var planCode = request.PlanCode?.Trim() ?? string.Empty;

        // Serializamos los intentos que comparten la misma clave de idempotencia
        // para que dos envíos idénticos simultáneos produzcan una sola venta.
        var lockKey = $"{advisorId}::{reference}";
        using var _ = await _registrationLock.AcquireAsync(lockKey, cancellationToken);

        // Idempotencia: ¿ya existe una venta para (asesor, referencia)?
        var existing = await _saleRepository.GetByAdvisorAndReferenceAsync(advisorId, reference, cancellationToken);
        if (existing is not null)
        {
            // Mismos datos de negocio -> devolvemos la venta previa sin consumir cupo.
            if (existing.MatchesBusinessData(planCode, buyerName, buyerEmail))
                return existing.ToDto();

            // Datos distintos -> se conserva la original y se rechaza el cambio.
            throw new ReferenceConflictException(reference);
        }

        // Reintentamos ante conflictos de concurrencia optimista sobre el cupo.
        for (var attempt = 1; ; attempt++)
        {
            var plan = await _planRepository.GetByCodeAsync(planCode, cancellationToken)
                       ?? throw new PlanNotFoundException(planCode);

            if (!plan.HasQuota)
                throw new NoQuotaAvailableException(plan.Name);

            // El dominio garantiza que el cupo nunca quede negativo.
            plan.ConsumeOneQuota();

            var sale = new Sale(advisorId, reference, plan, buyerName, buyerEmail);
            await _saleRepository.AddAsync(sale, cancellationToken);

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return sale.ToDto();
            }
            catch (ConcurrencyConflictException) when (attempt < MaxConcurrencyRetries)
            {
                // Otro asesor tomó el cupo entre la lectura y la escritura.
                // Reintentamos con el estado fresco del plan.
            }
        }
    }

    public async Task<PagedResult<SaleListItemDto>> GetOwnSalesAsync(
        string advisorId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(advisorId))
            throw new InvalidSaleDataException("El asesor activo es obligatorio.");

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > MaxPageSize ? MaxPageSize : pageSize;

        // El filtro por asesor vive en el backend: no se puede ampliar
        // manipulando parámetros desde el cliente (R4).
        var (items, total) = await _saleRepository.GetByAdvisorPagedAsync(
            advisorId, page, pageSize, cancellationToken);

        var dtos = items.Select(s => s.ToListItemDto()).ToList();
        return new PagedResult<SaleListItemDto>(dtos, page, pageSize, total);
    }
}
