using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entity
{

    [Table("sys_role_menu")]
    public class SysRoleMenu : BaseEntity
    {
        [Key]
        public long? Id { get; set; }

        [Column("role_id")]
        public long? RoleId { get; set; }

        [Column("menu_id")]
        public long? MenuId { get; set; }
    }
}
