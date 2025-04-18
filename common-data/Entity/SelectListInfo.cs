using Core.Entity;
using System.ComponentModel;

namespace Data.Entity
{
    public class SelectListInfo<T> : INotifyPropertyChanged where T : BaseEntity
    {

        public T? Row { get; set; }
        public long? RowId { get; set; }

        public bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public SelectListInfo(long? id, T entity)
        {
            RowId = id;
            Row = entity;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
