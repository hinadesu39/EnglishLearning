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
    public record Album 
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        

        /// <summary>
        /// 用户是否可见（完善后才显示，或者已经显示了，但是发现内部有问题，就先隐藏，调整了再发布）
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// 标题
        /// </summary>
        public MultilingualString Name { get; private set; }

        /// <summary>
        /// 列表中的显示序号
        /// </summary>
        public int SequenceNumber { get; private set; }

        public Guid CategoryId { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime CreationTime { get; private set; } = DateTime.Now;
        public DateTime? DeletionTime { get; private set; }
        public DateTime? LastModificationTime { get; private set; }

        private Album() { }

        public static Album Create(Guid id, int sequenceNumber, MultilingualString name, Guid categoryId)
        {
            Album album = new Album();
            album.Id = id;
            album.SequenceNumber = sequenceNumber;
            album.Name = name;
            album.CategoryId = categoryId;
            album.IsVisible = false;//Album新建以后默认不可见，需要手动Show
            return album;
        }
        public Album ChangeSequenceNumber(int value)
        {
            this.SequenceNumber = value;
            return this;
        }

        public Album ChangeName(MultilingualString value)
        {
            this.Name = value;
            return this;
        }
        public Album Hide()
        {
            this.IsVisible = false;
            return this;
        }
        public Album Show()
        {
            this.IsVisible = true;
            return this;
        }
        public  void SoftDelete()
        {
            this.IsDeleted = true;
            this.DeletionTime = DateTime.Now;
        }

        public void NotifyModified()
        {
            this.LastModificationTime = DateTime.Now;
        }

        /// <summary>
        /// 领域事件相关
        /// </summary>
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

