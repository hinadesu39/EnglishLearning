using MediaEncoderDomain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaEncoderInfrastructure.Configs
{
    class EncodingItemConfig : IEntityTypeConfiguration<EncodingItem>
    {
        public void Configure(EntityTypeBuilder<EncodingItem> builder)
        {
            builder.ToTable("T_ME_EncodingItems");
            //todo:id需要非聚集索引。
            //todo:符合索引。
            builder.Property(e => e.Name).HasMaxLength(256);
            builder.Property(e => e.FileSHA256Hash).HasMaxLength(64).IsUnicode(false);
            builder.Property(e => e.OutputFormat).HasMaxLength(10).IsUnicode(false);
            builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(10);
        }
    }
}
