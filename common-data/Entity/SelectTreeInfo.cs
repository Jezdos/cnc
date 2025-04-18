using Core.Entity;
using Core.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Data.Entity
{
    /// <summary>
    /// Defines the <see cref="SelectTreeInfo{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SelectTreeInfo<T> : TreeEntity, INotifyPropertyChanged where T : TreeEntity
    {
        /// <summary>
        /// Gets or sets the Row
        /// </summary>
        public T? Row { get; set; }

        /// <summary>
        /// Gets or sets the RowId
        /// </summary>
        public long? RowId { get; set; }

        public bool _isSelected;

        /// <summary>
        /// Gets or sets a value indicating whether IsSelected
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                if (Parent != null && !Parent.IsSelected && value)
                {
                    Parent.IsSelected = true;
                }

                if (Children != null)
                {
                    if (value && Children.Any(child => child.IsSelected)) return;

                    foreach (var item in Children)
                    {
                        item.IsSelected = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the Parent
        /// </summary>
        public SelectTreeInfo<T>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the Children
        /// </summary>
        public ObservableCollection<SelectTreeInfo<T>>? Children { get; set; }

        /// <summary>
        /// Defines the PropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The OnPropertyChanged
        /// </summary>
        /// <param name="propertyName">The propertyName<see cref="string"/></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// The Build
        /// </summary>
        /// <param name="rows">The rows<see cref="List{T}"/></param>
        /// <param name="root">The root<see cref="long?"/></param>
        /// <param name="selected">The selected<see cref="List{long?}"/></param>
        /// <returns>The <see cref="List{SelectTreeInfo{T}}"/></returns>
        public static List<SelectTreeInfo<T>> Build(List<T> rows, long? root = 0, List<long?> selected = null)
        {
            var result = rows.Where(m => m.ParentId == root).OrderBy(m => m.Seq)
                .Select(m =>
                {
                    SelectTreeInfo<T> info = MapperUtil.Map<T, SelectTreeInfo<T>>(m);
                    info.RowId = m.GetId();
                    info.Row = m;
                    if (selected != null && info.RowId != null && selected.Contains(info.RowId.Value)) info._isSelected = true;
                    List<SelectTreeInfo<T>> list = Build(rows, m.GetId(), selected);
                    if (list.Any())
                    {
                        list.ForEach(m => m.Parent = info);
                        info.Children = new ObservableCollection<SelectTreeInfo<T>>(list);
                    }
                    return info;
                }).ToList();
            return result;
        }

        /// <summary>
        /// The GetAllNodes
        /// </summary>
        /// <returns>The <see cref="IEnumerable{SelectTreeInfo{T}}"/></returns>
        public IEnumerable<SelectTreeInfo<T>> GetAllNodes()
        {
            yield return this;
            if (Children != null)
                foreach (var child in Children)
                {
                    foreach (var node in child.GetAllNodes())
                    {
                        yield return node;
                    }
                }
        }

        /// <summary>
        /// The GetId
        /// </summary>
        /// <returns>The <see cref="long?"/></returns>
        public override long? GetId()
        {
            return RowId;
        }
    }
}
