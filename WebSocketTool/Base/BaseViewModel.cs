using System.ComponentModel;
using System.Runtime.CompilerServices;
using WebSocketTool.Annotations;

namespace WebSocketTool.Base
{
    public class BaseViewModel : ObservableObject
    {
        
    }

    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}