using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entity
{
    [Table("sys_role")]
    public class SysRole : BaseEntity
    {

        [Key]
        [Column("role_id")]
        public long? RoleId { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }
    }
}
