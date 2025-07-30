using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lboto.Helpers
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class PauseData : INotifyPropertyChanged
    {
        private int _pause;
        private int _randomRolled;
        private Range _range;
        private string _type;

        public string Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        public int RandomRolled
        {
            get => _randomRolled;
            set => SetField(ref _randomRolled, value);
        }

        public int Pause
        {
            get => _pause;
            set => SetField(ref _pause, value);
        }

        public Range Range
        {
            get => _range;
            set => SetField(ref _range, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PauseData(string type, int rolled, int pause, Range range)
        {
            _type = type;
            _randomRolled = rolled;
            _pause = pause;
            _range = range;
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
