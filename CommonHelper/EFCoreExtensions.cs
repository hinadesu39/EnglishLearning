using CommonHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class EFCoreExtensions
    {

        /// <summary>
        /// set global 'IsDeleted=false' queryfilter for every entity
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void EnableSoftDeletionGlobalFilter(this ModelBuilder modelBuilder)
        {
            var entityTypesHasSoftDeletion = modelBuilder.Model.GetEntityTypes()
                .Where(e => e.ClrType.IsAssignableTo(typeof(ISoftDelete)));

            foreach (var entityType in entityTypesHasSoftDeletion)
            {
                var isDeletedProperty = entityType.FindProperty(nameof(ISoftDelete.IsDeleted));
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var filter = Expression.Lambda(Expression.Not(Expression.Property(parameter, isDeletedProperty.PropertyInfo)), parameter);
                entityType.SetQueryFilter(filter);
            }
        }
        //这段代码定义了一个名为 "EnableSoftDeletionGlobalFilter" 的扩展方法，该方法接受一个 "ModelBuilder" 类型的参数。
        //该方法首先使用 LINQ 查询获取模型中所有实现了 "ISoftDelete" 接口的实体类型。
        //然后，对于每个实体类型，该方法使用表达式树构建一个查询过滤器，该过滤器检查实体的 "IsDeleted" 属性是否为 "false"。
        //最后，该方法使用 "SetQueryFilter" 方法为每个实体类型设置查询过滤器。
        //这个方法的目的是为模型中所有实现了 "ISoftDelete" 接口的实体类型启用软删除全局过滤器。
        //当启用了软删除全局过滤器后，查询这些实体类型时，将自动过滤掉已删除的实体（即 "IsDeleted" 属性为 "true" 的实体）。
    }
}
