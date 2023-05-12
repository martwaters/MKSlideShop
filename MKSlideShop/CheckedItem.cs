using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MKSlideShop
{
    public class CheckedItem : INotifyPropertyChanged
    {
        #region PropertyChangeHandler

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // PropertyChangeHandler

        #region Properties
        /// <summary>
        /// Start button state
        /// </summary>
        private bool use = true;
        public bool Use
        {
            get { return use; }
            set
            {
                use = value;
                OnPropertyChanged();
            }
        }

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(); }
        }

        #endregion // Properties

    }

}
