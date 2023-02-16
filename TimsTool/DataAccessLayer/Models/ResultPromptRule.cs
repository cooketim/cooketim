using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ResultPromptRule
{
    public Guid Uuid { get; set; }

    public Guid ResultDefinitionUuid { get; set; }

    public Guid ResultPromptUuid { get; set; }

    public string Rule { get; set; } = null!;
    public string RuleMags { get; set; } = null!;
    public string RuleCrown { get; set; } = null!;

    public int? PromptSequence { get; set; }

    public string? UserGroups { get; set; }

    public Guid MasterUuid { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string CreatedUser { get; set; } = null!;

    public string LastModifiedUser { get; set; } = null!;

    public string? DeletedUser { get; set; }

    public string? PublishedStatus { get; set; }

    public DateTime? PublishedStatusDate { get; set; }

    public string? PublicationTags { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
