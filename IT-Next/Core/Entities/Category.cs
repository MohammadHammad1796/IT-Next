﻿using System.ComponentModel.DataAnnotations;

namespace IT_Next.Core.Entities;

public class Category : EntityWithUniqueName
{
    [Required]
    [MaxLength(100)]
    public override string Name { get; set; }

    public ICollection<SubCategory> SubCategories { get; set; }

    public Category()
    {
        SubCategories = new List<SubCategory>();
    }
}