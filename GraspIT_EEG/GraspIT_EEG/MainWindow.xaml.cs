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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Threading;

// Metro Library.
using MahApps.Metro.Controls;

// Emotiv Libraries.
using Emotiv;
using EmoEngineClientLibrary;
using EmoEngineControlLibrary;

namespace GraspIT_EEG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        #region Emotiv

        EmoEngineClient _emoEngineClient;
        private EmoEngine engine;

        #endregion Emotiv

        #region Timers

        #region P300 Timers Declaration

        DispatcherTimer P300FlashDuration = new DispatcherTimer();
        DispatcherTimer P300NoFlashDuration = new DispatcherTimer();
        DispatcherTimer P300FlashingPeriod = new DispatcherTimer();

        #endregion P300 Timers Declaration

        #endregion Timers

        public MainWindow()
        {
            InitializeComponent();
            P300FlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 125);
            P300FlashDuration.Tick += P300FlashDuration_Tick;

            P300NoFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 475);
            P300NoFlashDuration.Tick += P300NoFlashDuration_Tick;

            P300FlashingPeriod.Interval = new TimeSpan(0, 0, 0, 0, 8);
            P300FlashingPeriod.Tick += P300FlashingPeriod_Tick;
        }

        #region P300 Timers Ticks

        void P300FlashingPeriod_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void P300NoFlashDuration_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void P300FlashDuration_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion P300 Timers Ticks

        #region Variables

        int currentPage = 1;

        #endregion Variables

        #region Animation Function.

        private void pageFadeOut(int currentPage)
        {
            switch (currentPage)
            {
                case 1:
                    Storyboard StartUnload = (Storyboard)FindResource("MainWindowPageOut");
                    StartUnload.Begin(this);
                    break;
                case 2:
                    Storyboard Step1Unload = (Storyboard)FindResource("SettingsPageOut");
                    Step1Unload.Begin(this);
                    break;
                default:
                    break;
            }
        }

        #endregion Animation Function

        #region Navigation Buttons

        private void ShowSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            pageFadeOut(currentPage);
            Storyboard Step1Load = (Storyboard)FindResource("SettingsPageIn");
            Step1Load.Begin(this);
            currentPage = 2;
        }

        private void BackToMainWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            pageFadeOut(currentPage);
            Storyboard Step1Load = (Storyboard)FindResource("MainWindowPageIn");
            Step1Load.Begin(this);
            currentPage = 1;
        }

        #endregion Navigation Buttons

        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EmotivToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            EmotivStatusLbl.Content = "Not Connected";
            //Battery.Source = new BitmapImage(new Uri ("Assets/Images/Battery/Battery-Empty.png", UriKind.Relative));
            UpdateBatteryCapacityIcon(0);
            UpdateSignalStrengthIcon(0);
        }

        private void EmotivToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // Try to connect the Emotiv device.

            EmotivStatusLbl.Content = "Connected";
            //Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-High.png", UriKind.Relative));
            UpdateBatteryCapacityIcon(3);
            UpdateSignalStrengthIcon(4);
        }


        /// <summary>
        /// Set the battery capacity based on the charge
        /// </summary>
        /// <param name="charge">The charge taking values: 0 -> empty, 1 -> low, 2 -> medium, 3-> high </param>
        private void UpdateBatteryCapacityIcon(int charge)
        {
            switch (charge)
            {
                // Empty
                case 0:
                Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Empty.png", UriKind.Relative));
                break;

                // Low
                case 1:
                Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Low.png", UriKind.Relative));
                break;

                // Medium
                case 2:
                Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Medium.png", UriKind.Relative));
                break;

                // High
                case 3:
                Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-High.png", UriKind.Relative));
                break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Set the signal capacity based on the strength
        /// </summary>
        /// <param name="strength">The strength taking values: 0 -> No Signal, 1 -> low, 2 -> medium low, 3-> medium high, 4 -> high </param>
        private void UpdateSignalStrengthIcon(int strength)
        {
            switch (strength)
            {
                // Empty
                case 0:
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - No Signal.png", UriKind.Relative));
                    break;

                // Low
                case 1:
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 1.png", UriKind.Relative));
                    break;

                // Medium low
                case 2:
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 2.png", UriKind.Relative));
                    break;

                // Medium high
                case 3:
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 3.png", UriKind.Relative));
                    break;
                
                // High
                case 4:
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 4.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
        }

        // Add User.
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        // Remove User.
        private void RemoveUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        // Save User.
        private void SaveUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        // Train User.
        private void TrainUserBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
