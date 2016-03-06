using CasperWP.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.System.Threading;



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CasperWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Drive : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private double XCoordinate = 0;
        private double YCoordinate = 0;

        private Socket socket;

        public Drive()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction) => ConnectionDelegate()));

            TimeSpan period = TimeSpan.FromMilliseconds(50);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((IAsyncAction) => StreamDelegate(), period);
        }

        private void ConnectionDelegate()
        {
            socket = new Socket("192.168.1.186", "9999", "9998");

            socket.TCPConnect();
            socket.UDPConnect();
        }

        private void StreamDelegate()
        {
            if (socket == null)
            {
                return;
            }
            else if (!socket.TCPConnected)
            {
                return;
            }
            else
            {
                Byte[] message = new Byte[8];

                message[0] = (Byte)'D';

                char driveFlag;
                if(YCoordinate>0)
                {
                    driveFlag = 'F';
                }
                else if(YCoordinate<0)
                {
                    driveFlag = 'B';
                    YCoordinate *= -1;
                }
                else
                {
                    driveFlag = 'I';
                }
                message[1] = (Byte)driveFlag;

                char steerFlag;
                if (XCoordinate > 0)
                {
                    steerFlag = 'R';
                }
                else if (XCoordinate < 0)
                {
                    steerFlag = 'L';
                    XCoordinate *= -1;
                }
                else
                {
                    steerFlag = 'I';
                }
                message[2] = (Byte)steerFlag;

                Byte Y = (Byte)(Math.Abs(YCoordinate) * 255);

                message[3] = Y;

                Byte X = (Byte)(Math.Abs(XCoordinate) * 255);

                message[4] = X;
                message[6] = 0xD;
                message[7] = 0xA;

                socket.SendMessage(message);          
            }
        }

        private void OnVideo(object sender, RoutedEventArgs e)
        {
            socket.StartVideo(videoView);
        }

        private async void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {      
            Point dragPoint = e.GetCurrentPoint(Board).Position;

            Canvas.SetLeft(joystickButton, dragPoint.X-25);
            Canvas.SetTop(joystickButton, dragPoint.Y-25);

            XCoordinate = (dragPoint.X - 75) / 75;
            YCoordinate = -1*(dragPoint.Y - 75) / 75;            
        }

        private void JoystickReleased(object sender, PointerRoutedEventArgs e)
        {
            Canvas.SetLeft(joystickButton, 37.5);
            Canvas.SetTop(joystickButton, 37.5);

            XCoordinate = 0;
            YCoordinate = 0;
        }

        private void LeftCanvas(object sender, PointerRoutedEventArgs e)
        {
            Canvas.SetLeft(joystickButton, 37.5);
            Canvas.SetTop(joystickButton, 37.5);

            XCoordinate = 0;
            YCoordinate = 0;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.LandscapeFlipped | DisplayOrientations.Landscape;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);

            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
        }

        #endregion
    }
}
