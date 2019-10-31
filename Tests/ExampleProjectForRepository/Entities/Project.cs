﻿using System;

using StandardRepository.Models.Entities;
using StandardRepository.Models.Entities.Schemas;

namespace ExampleProjectForRepository.Entities
{
    public class Project : BaseEntity, ISchemaDomain
    {
        public long OrganizationId { get; set; }
        public Guid OrganizationUid { get; set; }
        public string OrganizationName { get; set; }

        public string Description { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public decimal ProjectCost { get; set; }
        
        public Guid? OwnerUid { get; set; }

        public object ProjectValue { get; set; }
    }
}