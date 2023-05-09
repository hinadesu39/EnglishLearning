using CommonHelper;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningDomain.Entities
{
    public record Category 
    {
        private Category() { }

        public Guid Id { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// 在所有Category中的显示序号，越小越靠前
        /// </summary>
        public int SequenceNumber { get; private set; }
        public MultilingualString Name { get; private set; }

        /// <summary>
        /// 封面图片。现在一般都不会直接把图片保存到数据库中（Blob），而是只是保存图片的路径。
        /// </summary>
        public Uri CoverUrl { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime CreationTime { get; private set; } = DateTime.Now;
        public DateTime? DeletionTime { get; private set; }
        public DateTime? LastModificationTime { get; private set; }

        public virtual void SoftDelete()
        {
            this.IsDeleted = true;
            this.DeletionTime = DateTime.Now;
        }

        public void NotifyModified()
        {
            this.LastModificationTime = DateTime.Now;
        }
        public static Category Create(Guid id, int sequenceNumber, MultilingualString name, Uri coverUrl)
        {
            Category category = new();
            category.Id = id;
            category.SequenceNumber = sequenceNumber;
            category.Name = name;
            category.CoverUrl = coverUrl;
            //category.AddDomainEvent(new CategoryCreatedEventArgs { NewObj = category });
            return category;
        }

        public Category ChangeSequenceNumber(int value)
        {
            this.SequenceNumber = value;
            return this;
        }

        public Category ChangeName(MultilingualString value)
        {
            this.Name = value;
            return this;
        }

        public Category ChangeCoverUrl(Uri value)
        {
            //todo: 做项目的时候，不管这个事件是否有被用到，都尽量publish。
            this.CoverUrl = value;
            return this;
        }
        [NotMapped]
        private List<INotification> domainEvents = new();

        public void AddDomainEvent(INotification eventItem)
        {
            domainEvents.Add(eventItem);
        }

        public void AddDomainEventIfAbsent(INotification eventItem)
        {
            if (!domainEvents.Contains(eventItem))
            {
                domainEvents.Add(eventItem);
            }
        }
        public void ClearDomainEvents()
        {
            domainEvents.Clear();
        }

        public IEnumerable<INotification> GetDomainEvents()
        {
            return domainEvents;
        }
    }
}
