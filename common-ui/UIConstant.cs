using Wpf.Ui;

namespace UI
{
    public class UIConstant
    {
        public static readonly SimpleContentDialogCreateOptions DEL_CONFIRM = new SimpleContentDialogCreateOptions()
        {
            Title = "删除确认?",
            Content = "确实要永久性的删除此数据吗？",
            PrimaryButtonText = "确认",
            CloseButtonText = "取消",
        };
    }
}
