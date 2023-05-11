using CommonHelper;
using ListeningDomain.Events;
using ListeningDomain.Subtitles;
using ListeningDomain.ValueObjects;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ListeningDomain.Entities
{
    public class Episode: IDomainEvents
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        private Episode() { }
        public int SequenceNumber { get; private set; }//序号
        public MultilingualString Name { get; private set; }//标题
        public Guid AlbumId { get; private set; }//专辑Id，因为Episode和Album都是聚合根，因此不能直接做对象引用。
        public Uri AudioUrl { get; private set; }//音频路径
        /// <summary>
        ///这是音频的实际长度（秒）
        ///因为IE、旧版Edge、部分手机内置浏览器(小米等)中对于部分音频,
        ///计算的duration以及currentTime和实际的不一致，因此需要根据服务器端
        ///计算出来的实际长度，在客户端做按比例校正
        ///所以服务器端需要储存这个，以便给到浏览器
        /// </summary>
        public double DurationInSecond { get; private set; }//音频时长（秒数）

        //因为启用了<Nullable>enable</Nullable>，所以string是不可空，Migration会默认这个，string?是可空
        public string Subtitle { get; private set; }//原文字幕内容
        public string SubtitleType { get; private set; }//原文字幕格式

        /// <summary>
        /// 用户是否可见（如果发现内部有问题，就先隐藏）
        /// </summary>
        public bool IsVisible { get; private set; }

        public bool IsDeleted { get; private set; }
        public DateTime CreationTime { get; private set; } = DateTime.Now;
        public DateTime? DeletionTime { get; private set; }
        public DateTime? LastModificationTime { get; private set; }



        public static Episode Create(Guid id, int sequenceNumber, MultilingualString name, Guid albumId, Uri audioUrl,
            double durationInSecond, string subtitleType, string subtitle)
        {
            var parser = SubtitleParserFactory.GetParser(subtitleType);
            if (parser == null)
            {
                throw new ArgumentOutOfRangeException(nameof(subtitleType), $"subtitleType={subtitleType} is not supported.");
            }

            //新建的时候默认可见
            Episode episode = new Episode()
            {
                Id = id,
                AlbumId = albumId,
                DurationInSecond = durationInSecond,
                AudioUrl = audioUrl,
                Name = name,
                SequenceNumber = sequenceNumber,
                Subtitle = subtitle,
                SubtitleType = subtitleType,
                IsVisible = true
            };
            episode.AddDomainEvent(new EpisodeCreatedEvent(episode));
            
            return episode;
        }
        public Episode ChangeSequenceNumber(int value)
        {
            this.SequenceNumber = value;
            this.AddDomainEventIfAbsent(new EpisodeUpdatedEvent(this));
            return this;
        }

        public Episode ChangeName(MultilingualString value)
        {
            this.Name = value;
            this.AddDomainEventIfAbsent(new EpisodeUpdatedEvent(this));
            return this;
        }

        public Episode ChangeSubtitle(string subtitleType, string subtitle)
        {
            var parser = SubtitleParserFactory.GetParser(subtitleType);
            if (parser == null)
            {
                throw new ArgumentOutOfRangeException(nameof(subtitleType), $"subtitleType={subtitleType} is not supported.");
            }
            this.SubtitleType = subtitleType;
            this.Subtitle = subtitle;
            this.AddDomainEventIfAbsent(new EpisodeUpdatedEvent(this));
            return this;
        }

        public Episode Hide()
        {
            this.IsVisible = false;
            this.AddDomainEventIfAbsent(new EpisodeUpdatedEvent(this));
            return this;
        }
        public Episode Show()
        {
            this.IsVisible = true;
            this.AddDomainEventIfAbsent(new EpisodeUpdatedEvent(this));
            return this;
        }

        public void SoftDelete()
        {
            this.IsDeleted = true;
            this.DeletionTime = DateTime.Now;
            this.AddDomainEvent(new EpisodeDeletedEvent(this.Id));
        }

        public IEnumerable<Sentence> ParseSubtitle()
        {
            var parser = SubtitleParserFactory.GetParser(this.SubtitleType);
            return parser.Parse(this.Subtitle);
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
