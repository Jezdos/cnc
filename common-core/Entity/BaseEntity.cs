using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entity
{
    public class BaseEntity
    {
        [Column("create_time")]
        public DateTime? CreateTime { get; set; }

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("create_by")]
        public string? CreateBy { get; set; }

        [Column("update_by")]
        public string? UpdateBy { get; set; }

        public string? Remark { get; set; }

    }
}
