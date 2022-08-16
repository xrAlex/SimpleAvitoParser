namespace Parser.Entities
{
    public sealed class Advertisement : IEquatable<Advertisement>
    {
        /// <summary>
        /// Данные пользователя выложевшего объявление
        /// </summary>
        public User? Owner { get; }

        /// <summary>
        /// Заголовок объявление
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Ссылка на объявление
        /// </summary>
        public string Link { get; }

        /// <summary>
        /// Ссылка на изображение объявления
        /// </summary>
        public string? ImageLink { get; }

        /// <summary>
        /// Локация объявления
        /// </summary>
        public string? Location { get; }

        /// <summary>
        /// Детальная информация о локации объявления
        /// </summary>
        public string? DetailedLocation { get; }

        /// <summary>
        /// Уникальный идентификатор объявления
        /// </summary>
        public long Identifier { get; }

        /// <summary>
        /// Цена указанная в объявлении
        /// </summary>
        public int? Cost { get; }

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public string? LastUpdate { get; }

        /// <summary>
        /// Описание объявления
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Категория объявления
        /// </summary>
        public string? Category { get; }

        /// <summary>
        /// Город указанный в объявлении
        /// </summary>
        public string? City { get; }

        /// <summary>
        /// Параметр указывающий была ли сделана скидка продавцом
        /// </summary>
        public bool? Sale { get; }

        /// <summary>
        /// Дата создания объявления
        /// </summary>
        public DateTime? CreationTime { get; }

        public bool Equals(Advertisement? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifier == other.Identifier;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Advertisement)obj);
        }

        public override int GetHashCode() 
            => Identifier.GetHashCode();

        internal Advertisement(string title, string link, long identifier,
            int? cost, string? lastUpdate, string? description, string? imageLink,
            string? location, string? detailedLocation, User? owner, string? category,
            bool? sale, string? city, DateTime? creationTime)
        {
            Title = title;
            Link = link;
            Identifier = identifier;
            Cost = cost;
            LastUpdate = lastUpdate;
            Description = description;
            ImageLink = imageLink;
            Location = location;
            DetailedLocation = detailedLocation;
            Owner = owner;
            Category = category;
            Sale = sale;
            City = city;
            CreationTime = creationTime;
        }
    }
}
