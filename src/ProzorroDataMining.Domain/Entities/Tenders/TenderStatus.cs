using System.Runtime.Serialization;

namespace ProzorroDataMining.Domain.Entities.Tenders;

public enum TenderStatus
{
    [EnumMember(Value = "unknown")]
    Unknown = 0,

    [EnumMember(Value = "draft")]
    Draft = 1,

    [EnumMember(Value = "active")]
    Active = 2,

    [EnumMember(Value = "active.enquiries")]
    ActiveEnquiries = 3,

    [EnumMember(Value = "active.tendering")]
    ActiveTendering = 4,

    [EnumMember(Value = "active.pre-qualification")]
    ActivePreQualification = 5,

    [EnumMember(Value = "active.pre-qualification.stand-still")]
    ActivePreQualificationStandStill = 6,

    [EnumMember(Value = "active.auction")]
    ActiveAuction = 7,

    [EnumMember(Value = "active.qualification")]
    ActiveQualification = 8,

    [EnumMember(Value = "active.awarded")]
    ActiveAwarded = 9,

    [EnumMember(Value = "active.stage2.pending")]
    ActiveStage2Pending = 10,

    [EnumMember(Value = "unsuccessful")]
    Unsuccessful = 11,

    [EnumMember(Value = "complete")]
    Complete = 12,

    [EnumMember(Value = "cancelled")]
    Cancelled = 13,
}
