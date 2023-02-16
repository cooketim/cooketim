using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class Comment
{
    public Guid Uuid { get; set; }

    public Guid MasterUuid { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public DateTime? DeletedDate { get; set; }

    public string CreatedUser { get; set; } = null!;

    public string LastModifiedUser { get; set; } = null!;

    public string? DeletedUser { get; set; }

    public string? PublishedStatus { get; set; }

    public DateTime? PublishedStatusDate { get; set; }

    public Guid ParentUuid { get; set; }

    public Guid ParentMasterUuid { get; set; }

    public string Note { get; set; } = null!;

    public string? SystemCommentType { get; set; }

    public string? PublicationTags { get; set; }

    public byte[] RowVersion { get; set; } = null!;
}
