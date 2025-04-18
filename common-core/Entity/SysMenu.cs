using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entity
{
    [Table("sys_menu")]
    public class SysMenu : TreeEntity
    {

        [Key]
        [Column("menu_id")]
        public long? MenuId { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Router { get; set; }
        public MenuPositionEnum Position { get; set; }

        public override long? GetId()
        {
            return MenuId;
        }

        public bool isRoot()
        {
            if (ParentId == 0) return true;
            return false;
        }
    }

    public enum MenuPositionEnum
    {
        TOP, BOTTOM
    }


}
