using System;
using System.Collections.Generic;
using System.Windows;

namespace WebSocketTool.View
{
    public class OWindow : Window
    {
        protected log4net.ILog log = log4net.LogManager.GetLogger(nameof(OWindow));
        public bool IsClosed { get; private set; } = false;
        public bool IsShowing { get { return (!IsClosed && Visibility == Visibility.Visible && IsLoaded); } }
        public static LinkedList<Window> sSavedWindows = new LinkedList<Window>();


        public OWindow(string name, bool isSave = false)
        {
            log = log4net.LogManager.GetLogger(GetType().Name);
            this.Name = name;
            if (isSave)
            {
                OWindow.sSavedWindows.AddLast(this);
            }
            Init();
        }

        public OWindow()
        {
            Init();
        }

        public void Init()
        {
            this.Closed += OnClosed;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        public virtual new void Show()
        {
            try
            {
                base.Show();
            }
            catch (Exception ex)
            {
                //handle exception when base window be closed 
                log.Error(string.Format("show window: {0} type: {1}, e: {2}", this.Name, this.GetType(), ex));
            }
        }

        public virtual new void Close()
        {
            try
            {
                IsClosed = true;
                base.Close();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Close window: {0} type: {1}, e: {2}", this.Name, this.GetType(), ex));
            }
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            IsClosed = true;
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e) { }
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e) { }

    }
}