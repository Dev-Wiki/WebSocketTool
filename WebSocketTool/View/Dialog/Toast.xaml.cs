using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WebSocketTool.View.Dialog
{
    /// <summary>
    /// Toast.xaml 的交互逻辑
    /// </summary>
    public partial class Toast : OWindow
    {
        public const long ShortTime = 3000;
        public const long LongTime = 5000;
        public const int DefaultOffset = 32;

        private readonly DispatcherTimer timer;
        public double Offset { get; private set; } = DefaultOffset;
        public ToastLocation ToastLocation { get; private set; } = ToastLocation.Top;
        public long ShowTime { get; private set; } = ShortTime;

        public Toast()
        {
            InitializeComponent();
            ShowInTaskbar = false;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(ShowTime);
            timer.Tick += (sender, args) =>
            {
                if (!IsClosed)
                {
                    Close();
                }
                timer.Stop();
            };
            Loaded += OnToastLoaded;
            Closed += (sender, args) =>
            {
                if (ToastLocation != ToastLocation.ScreenCenter)
                {
                    Owner.LocationChanged -= UpdateLocation;
                    Owner.SizeChanged -= UpdateLocation;
                    Owner.StateChanged -= UpdateLocation;
                }

                if (timer != null && timer.IsEnabled)
                {
                    timer.Stop();
                }
            };
        }

        private void OnToastLoaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
            UpdateLocation(null, null);
            if (ToastLocation != ToastLocation.ScreenCenter)
            {
                Owner.LocationChanged += UpdateLocation;
                Owner.SizeChanged += UpdateLocation;
                Owner.StateChanged += UpdateLocation;
            }
        }

        public void UpdateOffset(double offset)
        {
            this.Offset = offset;
            UpdateLocation(null, null);
        }

        private void UpdateLocation(object sender, EventArgs e)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            if (ToastLocation == ToastLocation.ScreenCenter)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                return;
            }
            if (Owner == null)
            {
                log.Info($"UpdateLocation Owner is NULL");
                return;
            }
            double ownerTop = Owner.Top;
            double ownerLeft = Owner.Left;
            if (Owner.WindowState == WindowState.Maximized)
            {
                ownerTop = 0;
                ownerLeft = 0;
            }
            if (ToastLocation == ToastLocation.Center)
            {
                Top = ownerTop + ((Owner.ActualHeight - ActualHeight) / 2);
                Left = ownerLeft + ((Owner.ActualWidth - ActualWidth) / 2);
            }
            else
            {
                Left = ownerLeft + ((Owner.ActualWidth - ActualWidth) / 2);
                if (ToastLocation == ToastLocation.Bottom)
                {
                    Top = ownerTop + Owner.ActualHeight - ActualHeight - Offset - Owner.BorderThickness.Bottom;
                }
                else
                {
                    if (Owner.WindowStyle == WindowStyle.None)
                    {
                        Top = ownerTop + Offset;
                    }
                    else
                    {
                        Top = ownerTop + Offset + SystemParameters.WindowCaptionHeight;
                    }
                }
            }
        }

        public void Create(Window owner, string message, ToastLocation toastLocation = ToastLocation.Top,
            long time = ShortTime, int locationOffset = DefaultOffset)
        {
            if (owner == null && toastLocation != ToastLocation.ScreenCenter)
            {
                return;
            }

            if (time <= 0)
            {
                time = ShortTime;
            }

            if (locationOffset <= 0)
            {
                locationOffset = DefaultOffset;
            }
            Offset = locationOffset;
            ShowTime = time;
            ToastLocation = toastLocation;
            if (owner != null && owner.IsLoaded)
            {
                Owner = owner;
            }
            HintTb.Text = message;
            if (owner != null) MaxWidth = owner.ActualWidth - 16;
            ShowActivated = false;
        }

        public static Toast CreateToast(Window owner, string message, ToastLocation location = ToastLocation.Top,
            long time = ShortTime, int offset = DefaultOffset)
        {
            var toast = new Toast();
            toast.Create(owner, message, location, time, offset);
            return toast;
        }

        public static void ShowShort(Window owner, string message, ToastLocation location = ToastLocation.Top)
        {
            Toast toast = CreateToast(owner, message, location);
            toast.Show();
        }

        public static void ShowLong(Window owner, string message, ToastLocation location = ToastLocation.Top)
        {
            Toast toast = CreateToast(owner, message, location, LongTime);
            toast.Show();
        }
    }

    /// <summary>
    /// Toast的垂直位置, 水平居中
    /// </summary>
    public enum ToastLocation
    {
        Top,
        Bottom,
        Center,
        ScreenCenter
    }
}
