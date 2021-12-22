using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OutOfSchool.Services.Models.Images;

namespace OutOfSchool.Services.Models.Configurations.Images
{
    internal class WorkshopImagesConfiguration : IEntityTypeConfiguration<Image<Workshop>>
    {
        public void Configure(EntityTypeBuilder<Image<Workshop>> builder)
        {
            builder.HasKey(nameof(Image<Workshop>.EntityId), nameof(Image<Workshop>.ExternalStorageId));

            builder
                .HasOne(x => x.Entity)
                .WithMany(x => x.WorkshopImages)
                .HasForeignKey(x => x.EntityId);
        }
    }
}
