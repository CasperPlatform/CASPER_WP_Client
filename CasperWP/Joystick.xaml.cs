using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace CasperWP
{
    public sealed partial class Joystick : UserControl
    {
        private UIElement draggedItem = null;
        private Point offset;

        public Joystick()
        {
            this.InitializeComponent();
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (draggedItem == null)
                return;

            Point dragPoint = e.GetCurrentPoint(Board).Position;
            Canvas.SetLeft(draggedItem, dragPoint.X - offset.X);
            Canvas.SetTop(draggedItem, dragPoint.Y - offset.Y);
        }

        private void JoystickPressed(object sender, PointerRoutedEventArgs e)
        {

            draggedItem = joystickButton;
            offset = e.GetCurrentPoint(draggedItem).Position;
        }

        private void JoystickReleased(object sender, PointerRoutedEventArgs e)
        {
           
        }
    }
}
