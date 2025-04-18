using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entity
{
    public abstract class TreeEntity : BaseEntity
    {

        [Column("parent_id")]
        public long? ParentId { get; set; }

        public int? Seq { get; set; }

        public abstract long? GetId();
    }
}
