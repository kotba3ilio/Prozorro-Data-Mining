namespace ProzorroDataMining.Domain.Entities.Tenders;

public sealed class TenderSupplier
{
    public Guid TenderId { get; set; }

    public Tender? Tender { get; set; }

    public Guid SupplierId { get; set; }

    public Supplier? Supplier { get; set; }

    public string? AwardId { get; set; }

    private TenderSupplier()
    {
    }

    private TenderSupplier(Tender tender, Supplier supplier, string? awardId)
    {
        TenderId = tender.Id;
        SupplierId = supplier.Id;
        Tender = tender;
        Supplier = supplier;
        AwardId = awardId ?? string.Empty;
    }

    public static TenderSupplier Create(Tender tender, Supplier supplier, string? awardId)
    {
        return new TenderSupplier(tender, supplier, awardId);
    }
}
