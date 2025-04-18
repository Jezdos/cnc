using CommunityToolkit.Mvvm.ComponentModel;
using Core.Handler;
using Core.Utils;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace APP.ViewModels;
public partial class ViewModelEditor(bool autoClose = true) : ObservableValidator
{
    public static readonly SuccessEventHandler DEFAULT_SUCCESS_HANDLER = async () =>
    {
        if (DialogHost.IsDialogOpen(AppConstants.DIALOG_ROOT))
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                DialogHost.Close(AppConstants.DIALOG_ROOT);
            });
        } 
    };

    private bool _autoClose = autoClose;

    private bool _isFree = true;
    public bool IsFree
    {
        get => _isFree;
        private set => SetProperty(ref _isFree, value);
    }

    private DateTime _lastExecuteTime = DateTime.MinValue;

    private CancellationTokenSource? _debounceCts;


    /// <summary>
    /// 核心方法：支持防重复、防抖、节流
    /// </summary>
    /// <param name="action">要执行的异步任务</param>
    /// <param name="throttleMilliseconds">节流时间（间隔内无法再次触发）</param>
    /// <param name="debounceMilliseconds">防抖时间（延迟后执行）</param>
    /// <param name="allowConcurrency">是否允许并发</param>
    /// <returns></returns>
    protected async Task ExecuteAsync(
        Func<Task<bool>> action,
        int throttleMilliseconds = 1000,
        int debounceMilliseconds = 100,
        bool allowConcurrency = false)
    {
        if (!allowConcurrency && !IsFree)
            return;

        // 节流限制
        if (throttleMilliseconds > 0 &&
            (DateTime.Now - _lastExecuteTime).TotalMilliseconds < throttleMilliseconds)
        {
            return;
        }

        // 防抖延迟
        if (debounceMilliseconds > 0)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            try
            {
                await Task.Delay(debounceMilliseconds, token);
            }
            catch (TaskCanceledException)
            {
                return; // 被取消了，代表还在等后一次点击
            }
        }

        IsFree = false;
        _lastExecuteTime = DateTime.Now;

        try
        {
            await action().ContinueWith(async (var) =>
            {
                if (_autoClose && var.Result) {
                    DEFAULT_SUCCESS_HANDLER.Invoke();
                    await Task.Delay(200);
                }
                
            });
        }
        finally
        {
            IsFree = true;
        }
    }
}
