using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;


namespace Data.Entity
{
    /// <summary>
    /// Defines the <see cref="PageViewModelBase{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PageViewModelBase<T> : ObservableObject
    {
        public ObservableCollection<T> _DataList;

        // 每页显示多少条
        [ObservableProperty]
        public int pageSize = 20;

        // 总条目数
        [ObservableProperty]
        public int totalCount = 0;

        // 当前页
        [ObservableProperty]
        public int pageIndex = 0;

        // 页面总数
        [ObservableProperty]
        public int totalPage = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageViewModelBase{T}"/> class.
        /// </summary>
        public PageViewModelBase()
        {
            _DataList = new ObservableCollection<T>();
        }

        /// <summary>
        /// Gets or sets the DataList
        /// </summary>
        public ObservableCollection<T> DataList { get => _DataList; set => SetProperty(ref _DataList, value); }

        /// <summary>
        /// The RefreshPageInfo
        /// </summary>
        /// <param name="items">The items<see cref="IList{T}"/></param>
        protected void RefreshPageInfo(IList<T> items)
        {
            if (DataList.Any())
            {
                DataList.Clear();
            }
            foreach (var item in items)
            {
                DataList.Add(item);
            }
        }

        /// <summary>
        /// The RefreshPageInfo
        /// </summary>
        /// <param name="pagedList">The pagedList<see cref="IPagedList{T}"/></param>
        protected void RefreshPageInfo(IPagedList<T> pagedList)
        {
            RefreshPageInfo(pagedList.Items);
            this.TotalCount = pagedList.TotalCount;
            this.PageIndex = pagedList.PageIndex;
            this.TotalPage = pagedList.TotalPages;
        }

        /// <summary>
        /// The RefreshPageInfo
        /// </summary>
        /// <typeparam name="A"></typeparam>
        /// <param name="pagedList">The pagedList<see cref="IPagedList{A}"/></param>
        /// <param name="items">The items<see cref="List{T}"/></param>
        protected void RefreshPageInfo<A>(IPagedList<A> pagedList, List<T> items)
        {
            RefreshPageInfo(items);
            this.TotalCount = pagedList.TotalCount;
            this.PageIndex = pagedList.PageIndex;
            this.TotalPage = pagedList.TotalPages;
        }
    }
}
