namespace ProzorroDataMining.Domain.Entities.Tenders;

public sealed class TenderItem : Entity
{
    private TenderItem()
    {
    }

    private TenderItem(string classificationId, string? description)
    {
        ClassificationId = classificationId;
        Description = description;
    }

    public Guid TenderId { get; private set; }

    public Tender? Tender { get; private set; }

    public string ClassificationId { get; private set; } = null!;

    public string? Description { get; private set; }

    public static TenderItem Create(string classificationId, string? description)
    {
        return new TenderItem(classificationId, description);
    }
}
