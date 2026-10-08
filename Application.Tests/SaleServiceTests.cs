using Application.DTOs;
using Domain.Exceptions;
using FluentAssertions;

namespace Application.Tests;

public class SaleServiceTests
{
    private static RegisterSaleRequest Request(
        string plan = "ACC_ESENCIAL",
        string name = "Juan Perez",
        string email = "juan@ejemplo.test",
        string reference = "REF-0001")
        => new(plan, name, email, reference);

    [Fact]
    public async Task Last_quota_contested_by_two_advisors_accepts_only_one()
    {
        // R3: dos asesores por el último cupo del plan Esencial (cupo = 1).
        await using var harness = await TestHarness.CreateAsync(esencialQuota: 1);

        var saleA = await harness.SaleService.RegisterSaleAsync(
            "A", Request(reference: "REF-A-1"));

        Func<Task> saleB = () => harness.SaleService.RegisterSaleAsync(
            "B", Request(reference: "REF-B-1"));

        saleA.Should().NotBeNull();
        await saleB.Should().ThrowAsync<NoQuotaAvailableException>();

        (await harness.QuotaOf("ACC_ESENCIAL")).Should().Be(0, "el cupo nunca queda negativo");
    }

    [Fact]
    public async Task Resending_same_reference_and_data_returns_previous_sale_without_consuming_quota()
    {
        // R3: repetición aceptada -> misma venta, mismo id, mismo cupo.
        await using var harness = await TestHarness.CreateAsync(plusQuota: 5);

        var first = await harness.SaleService.RegisterSaleAsync(
            "A", Request(plan: "ACC_PLUS", reference: "REF-A-7"));

        var quotaAfterFirst = await harness.QuotaOf("ACC_PLUS");

        var second = await harness.SaleService.RegisterSaleAsync(
            "A", Request(plan: "ACC_PLUS", reference: "REF-A-7"));

        second.Id.Should().Be(first.Id);
        second.Premium.Should().Be(first.Premium);
        (await harness.QuotaOf("ACC_PLUS")).Should().Be(quotaAfterFirst, "el reenvío no consume otro cupo");
    }

    [Fact]
    public async Task Reusing_reference_with_different_data_is_rejected_and_keeps_original()
    {
        // R3: referencia aceptada reenviada con otros datos -> rechazo, se conserva la original.
        await using var harness = await TestHarness.CreateAsync(plusQuota: 5);

        var original = await harness.SaleService.RegisterSaleAsync(
            "A", Request(plan: "ACC_PLUS", name: "Juan Perez", reference: "REF-A-9"));

        Func<Task> changed = () => harness.SaleService.RegisterSaleAsync(
            "A", Request(plan: "ACC_PLUS", name: "Pedro Gomez", reference: "REF-A-9"));

        await changed.Should().ThrowAsync<ReferenceConflictException>();

        var stored = await harness.SaleService.GetOwnSalesAsync("A", 1, 20);
        stored.Items.Should().ContainSingle(s => s.Id == original.Id);
        stored.Items.Should().OnlyContain(s => s.BuyerName == "Juan Perez");
    }

    [Fact]
    public async Task Rejected_sale_due_to_no_quota_does_not_modify_quota_or_create_sale()
    {
        // R3: rechazo por falta de cupos no consume cupos ni deja venta parcial.
        await using var harness = await TestHarness.CreateAsync(esencialQuota: 0);

        Func<Task> attempt = () => harness.SaleService.RegisterSaleAsync(
            "A", Request(reference: "REF-A-X"));

        await attempt.Should().ThrowAsync<NoQuotaAvailableException>();

        (await harness.QuotaOf("ACC_ESENCIAL")).Should().Be(0);
        var sales = await harness.SaleService.GetOwnSalesAsync("A", 1, 20);
        sales.TotalCount.Should().Be(0, "un rechazo no deja venta aceptada parcial");
    }

    [Fact]
    public async Task Invalid_email_is_rejected_and_does_not_consume_quota()
    {
        await using var harness = await TestHarness.CreateAsync(esencialQuota: 1);

        Func<Task> attempt = () => harness.SaleService.RegisterSaleAsync(
            "A", Request(email: "correo-invalido", reference: "REF-A-BAD"));

        await attempt.Should().ThrowAsync<InvalidSaleDataException>();
        (await harness.QuotaOf("ACC_ESENCIAL")).Should().Be(1);
    }

    [Fact]
    public async Task Advisors_are_isolated_each_sees_only_own_sales()
    {
        // R4: A y B pueden reutilizar el mismo texto de referencia; cada uno
        // ve solo sus propias ventas.
        await using var harness = await TestHarness.CreateAsync(plusQuota: 5);

        await harness.SaleService.RegisterSaleAsync("A", Request(plan: "ACC_PLUS", reference: "REF-SHARED"));
        await harness.SaleService.RegisterSaleAsync("B", Request(plan: "ACC_PLUS", reference: "REF-SHARED"));

        var salesA = await harness.SaleService.GetOwnSalesAsync("A", 1, 20);
        var salesB = await harness.SaleService.GetOwnSalesAsync("B", 1, 20);

        salesA.TotalCount.Should().Be(1);
        salesB.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Premium_is_resolved_from_plan_ignoring_client_values()
    {
        // La prima proviene del plan, no del cliente.
        await using var harness = await TestHarness.CreateAsync(plusQuota: 5);

        var sale = await harness.SaleService.RegisterSaleAsync(
            "A", Request(plan: "ACC_PLUS", reference: "REF-PREMIUM"));

        sale.Premium.Should().Be(90_000m);
    }

    [Fact]
    public async Task Page_size_is_capped_at_20()
    {
        await using var harness = await TestHarness.CreateAsync(plusQuota: 5);

        var result = await harness.SaleService.GetOwnSalesAsync("A", 1, 500);
        result.PageSize.Should().Be(20);
    }
}
