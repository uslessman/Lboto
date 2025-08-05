using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lboto.Helpers
{
    public class Range : INotifyPropertyChanged
    {
        private int _min;
        private int _max;

        public int Min
        {
            get => _min;
            set => SetField(ref _min, value);
        }

        public int Max
        {
            get => _max;
            set => SetField(ref _max, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Range(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public override string ToString() => $"{Min}-{Max}";

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
