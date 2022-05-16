namespace RecruitmentApp.Core.Models;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
}
