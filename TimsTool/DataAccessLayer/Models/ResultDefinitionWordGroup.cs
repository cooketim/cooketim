﻿using System;
using System.Collections.Generic;

namespace DataAccessLayer.Models;

public partial class ResultDefinitionWordGroup
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

    public string? PublicationTags { get; set; }

    public string Payload { get; set; } = null!;

    public byte[] RowVersion { get; set; } = null!;
}
