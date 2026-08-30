namespace ProzorroDataMining.Domain.Entities.Tenders;

public sealed class Tender : Entity
{
    private Tender()
    {
    }

    private Tender(
        string prozorroId,
        TenderStatus status,
        DateTimeOffset? dateCreated,
        string procuringEntityName,
        decimal expectedAmount,
        string? currency,
        DateTimeOffset importedAt)
    {
        ProzorroId = prozorroId;
        ImportedAt = importedAt.ToUniversalTime();

        UpdateDetails(
            status,
            dateCreated,
            procuringEntityName,
            expectedAmount,
            currency,
            importedAt);
    }

    public string ProzorroId { get; private set; } = null!;

    public TenderStatus Status { get; private set; }

    public DateTimeOffset? DateCreated { get; private set; }

    public string ProcuringEntityName { get; private set; } = null!;

    public decimal ExpectedAmount { get; private set; }

    public string? Currency { get; private set; }

    public DateTimeOffset ImportedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public ICollection<TenderItem> Items { get; private set; } = [];

    public ICollection<TenderContract> Contracts { get; private set; } = [];

    public ICollection<TenderSupplier> Suppliers { get; private set; } = [];

    public static Tender Create(
        string prozorroId,
        TenderStatus status,
        DateTimeOffset? dateCreated,
        string procuringEntityName,
        decimal expectedAmount,
        string? currency,
        DateTimeOffset importedAt)
    {
        return new Tender(
            prozorroId,
            status,
            dateCreated,
            procuringEntityName,
            expectedAmount,
            currency,
            importedAt);
    }

    public void UpdateDetails(
        TenderStatus status,
        DateTimeOffset? dateCreated,
        string procuringEntityName,
        decimal expectedAmount,
        string? currency,
        DateTimeOffset updatedAt)
    {
        Status = status;
        DateCreated = dateCreated?.ToUniversalTime();
        ProcuringEntityName = procuringEntityName;
        ExpectedAmount = expectedAmount;
        Currency = currency;
        UpdatedAt = updatedAt.ToUniversalTime();
    }

    public void ReplaceItems(IEnumerable<TenderItem> items)
    {
        Items.Clear();

        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    public void ReplaceContracts(IEnumerable<TenderContract> contracts)
    {
        Contracts.Clear();

        foreach (var contract in contracts)
        {
            Contracts.Add(contract);
        }
    }

    public void ReplaceSuppliers(IEnumerable<TenderSupplier> suppliers)
    {
        Suppliers.Clear();

        foreach (var supplier in suppliers)
        {
            Suppliers.Add(supplier);
        }
    }
}
