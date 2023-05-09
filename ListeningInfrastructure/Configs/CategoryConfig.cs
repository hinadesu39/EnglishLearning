using ListeningDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningInfrastructure.Configs
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("T_Categories");
            builder.HasKey(e => e.Id).IsClustered(false);//对于Guid主键，不要建聚集索引，否则插入性能很差
            //配置值类型
            builder.OwnsOne(e => e.Name, nv =>
            {
                nv.Property(c => c.Chinese).IsRequired().HasMaxLength(200).IsUnicode();
                nv.Property(c => c.English).IsRequired().HasMaxLength(200).IsUnicode();
            });
            //将为这两个属性创建一个复合索引，以提高查询性能。
            builder.Property(e => e.CoverUrl).IsRequired(false).HasMaxLength(500).IsUnicode();
        }
    }
}
