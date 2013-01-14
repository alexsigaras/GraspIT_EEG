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

// Metro Library
using MahApps.Metro.Controls;

// Emotiv Libraries
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

        #region UI Specific

        #region Variables

        int currentPage = 1;

        #endregion Variables

        #region Animation Function

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
            // Display a modal window with help how to use the application.
        }

        #endregion UI Specific

        #region Emotiv

        #region Device

        // Emotiv Engine
        EmoEngine engine = EmoEngine.Instance;

        // Emotiv Wireless Signal
        public string SignalStatus = "NO_SIGNAL";

        // Emotiv Battery
        public int BatteryLevel = 0;
        public int MaxBatteryLevel = 5;

        // Channels
        public double AF3, F7, F3, FC5, T7, P7, O1, O2, P8, T8, FC6, F4, F8, AF4;

        // Gyros
        public double GYROX, GYROY;

        int xmax = 0, ymax = 0;
        bool allow = false;

        // Others
        public double COUNTER, ES_TIMESTAMP, FUNC_ID, FUNC_VALUE, INTERPOLATED, MARKER, RAW_CQ, SYNC_SIGNAL, TIMESTAMP;

        float bufferSize;
        uint samplingRate;

        #endregion Device

        #region User

        // User ID
        uint userID = 0;

        #endregion User

        #region Expressiv (EMG Signals)

        float clench;
        float smile;
        float eyeXCoordinate;
        float eyeYCoordinate;
        float eyebrows;

        #endregion Expressiv (EMG Signals)

        #region Affectiv

        #endregion Affectiv

        #region Cognitiv

        #endregion Cognitiv

        #endregion Emotiv

        #region Timers

        DispatcherTimer gyrotimer = new DispatcherTimer();

        #region P300 Timers Declaration

        DispatcherTimer P300FlashDuration = new DispatcherTimer();
        DispatcherTimer P300NoFlashDuration = new DispatcherTimer();
        DispatcherTimer P300FlashingPeriod = new DispatcherTimer();

        #endregion P300 Timers Declaration

        #region SSVEP Timers Declaration

        DispatcherTimer SSVEPFlashDuration = new DispatcherTimer();
        DispatcherTimer SSVEPNoFlashDuration = new DispatcherTimer();
        DispatcherTimer SSVEPFlashingPeriod = new DispatcherTimer();

        #endregion SSVEP Timers Declaration

        #endregion Timers

        public MainWindow()
        {
            InitializeComponent();

            #region Instantiate Timers

            gyrotimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            gyrotimer.Tick += gyrotimer_Tick;

            P300FlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 125);
            P300FlashDuration.Tick += P300FlashDuration_Tick;

            P300NoFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 475);
            P300NoFlashDuration.Tick += P300NoFlashDuration_Tick;

            P300FlashingPeriod.Interval = new TimeSpan(0, 0, 0, 0, 8);
            P300FlashingPeriod.Tick +=P300FlashingPeriod_Tick;

            SSVEPFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 125);
            SSVEPFlashDuration.Tick += SSVEPFlashDuration_Tick;

            SSVEPNoFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 475);
            SSVEPNoFlashDuration.Tick += SSVEPNoFlashDuration_Tick;

            SSVEPFlashingPeriod.Interval = new TimeSpan(0, 0, 0, 0, 8);
            SSVEPFlashingPeriod.Tick += SSVEPFlashingPeriod_Tick;

            #endregion Instantiate Timers
        }

        #region Timer Ticks

        void gyrotimer_Tick(object sender, EventArgs e)
        {
            engine.ProcessEvents(1000);

            if ((int)userID == -1)
                return;

            if (allow)
            {
                int x = 0, y = 0;
                engine.HeadsetGetGyroDelta(userID, out x, out y);

                xmax += x;
                ymax += y;
                xValue.Content = xmax.ToString();
                yValue.Content = ymax.ToString();
            }

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);

            if (data == null)
            {
                return;
            }

            // Update Emotiv Sensor Data
            int _bufferSize = data[EdkDll.EE_DataChannel_t.TIMESTAMP].Length;
            for (int i = 0; i < _bufferSize; i++)
            {
                UpdateEmotivSensorData(data, i);
            }
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

        #region SSVEP Timers Ticks

        void SSVEPFlashingPeriod_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void SSVEPNoFlashDuration_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void SSVEPFlashDuration_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion SSVEP Timers Ticks

        #endregion Timer Ticks

        #region Emotiv Event Handlers

        void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            EmoState es = e.emoState;
            float elapsed = es.GetTimeFromStart();
            if ((elapsed > 5) && (!allow))
            {
                allow = true;
            }
            if (es.GetWirelessSignalStatus() == EdkDll.EE_SignalStrength_t.NO_SIGNAL)
            {
                allow = false;
            }

            SignalStatus = es.GetWirelessSignalStatus().ToString();
            Uptime.Content = ConvertToTime(es.GetTimeFromStart());
            es.GetBatteryChargeLevel(out BatteryLevel, out MaxBatteryLevel);
            UpdateSensorContactQuality(es);

            float leftEye, rightEye;
            EdkDll.EE_ExpressivAlgo_t lowerFaceAction, upperFaceAction;
            bool expressivIsActive, expressivIsBlink, expressivIsEyesOpen, expressivIsLeftWink, expressivIsLookingDown, expressivIsLookingLeft, expressivIsLookingRight, expressivIsLookingUp, expressivIsRightWink;
            float lowerFaceActionPower, upperFaceActionPower;

            EdkDll.EE_DataGetBufferSizeInSec(out bufferSize);
            EdkDll.EE_DataGetSamplingRate(userID, out samplingRate);

            es.ExpressivGetEyelidState(out leftEye, out rightEye);
            lowerFaceAction = es.ExpressivGetLowerFaceAction();
            lowerFaceActionPower = es.ExpressivGetLowerFaceActionPower();
            upperFaceAction = es.ExpressivGetUpperFaceAction();
            upperFaceActionPower = es.ExpressivGetUpperFaceActionPower();
            //expressivIsActive = es.ExpressivIsActive(
            expressivIsBlink = es.ExpressivIsBlink();
            expressivIsEyesOpen = es.ExpressivIsEyesOpen();
            expressivIsLeftWink = es.ExpressivIsLeftWink();
            expressivIsLookingDown = es.ExpressivIsLookingDown();
            expressivIsLookingLeft = es.ExpressivIsLookingLeft();
            expressivIsLookingRight = es.ExpressivIsLookingRight();
            expressivIsLookingUp = es.ExpressivIsLookingUp();
            expressivIsRightWink = es.ExpressivIsRightWink();

            EdkDll.EE_CognitivAction_t EEGAction;

            EEGAction = es.CognitivGetCurrentAction();

            bool cognitivIsNoisy;

            cognitivIsNoisy = es.CognitivIsActive();

            clench = es.ExpressivGetClenchExtent();
            smile = es.ExpressivGetSmileExtent();
            eyebrows = es.ExpressivGetEyebrowExtent();
            es.ExpressivGetEyeLocation(out eyeXCoordinate, out eyeYCoordinate);

            Eyebrows.Content = eyebrows.ToString();
            if (eyebrows > 0.10)
            {
                EyebrowRect.Fill = Brushes.Green;
            }
            else
            {
                EyebrowRect.Fill = Brushes.Red;
            }

            Clench.Content = clench.ToString();
            if (clench > 0.10)
            {
                ClenchRect.Fill = Brushes.Green;
            }
            else
            {
                ClenchRect.Fill = Brushes.Red;
            }

            SamplingRateLbl.Content = samplingRate.ToString();
            BufferSizeLbl.Content = bufferSize.ToString();

        }

        private void UpdateSensorContactQuality(EmoState es)
        {
            EdkDll.EE_EEG_ContactQuality_t[] contactQualityArray = es.GetContactQualityFromAllChannels();
            AF3Contact.Fill = getContactQualityColor(contactQualityArray[3].ToString());
            AF4Contact.Fill = getContactQualityColor(contactQualityArray[16].ToString());
            F7Contact.Fill = getContactQualityColor(contactQualityArray[4].ToString());
            F3Contact.Fill = getContactQualityColor(contactQualityArray[5].ToString());
            F4Contact.Fill = getContactQualityColor(contactQualityArray[14].ToString());
            F8Contact.Fill = getContactQualityColor(contactQualityArray[15].ToString());
            FC5Contact.Fill = getContactQualityColor(contactQualityArray[6].ToString());
            FC6Contact.Fill = getContactQualityColor(contactQualityArray[13].ToString());
            T7Contact.Fill = getContactQualityColor(contactQualityArray[7].ToString());
            T8Contact.Fill = getContactQualityColor(contactQualityArray[12].ToString());
            CMSContact.Fill = getContactQualityColor(contactQualityArray[0].ToString());
            DRLContact.Fill = getContactQualityColor(contactQualityArray[1].ToString());
            P7Contact.Fill = getContactQualityColor(contactQualityArray[8].ToString());
            P8Contact.Fill = getContactQualityColor(contactQualityArray[11].ToString());
            O1Contact.Fill = getContactQualityColor(contactQualityArray[9].ToString());
            O2Contact.Fill = getContactQualityColor(contactQualityArray[10].ToString());
        }

        

        void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            //   Console.WriteLine("User Added Event has occured");

            // record the user 
            userID = e.userId;

            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);

            // ask for up to 1 second of buffered data
            engine.EE_DataSetBufferSizeInSec(1);

        }

        #endregion Emotiv Event Handlers



        #region P300

        #endregion P300

        #region SSVEP

        #endregion SSVEP

        #region StatusBar

        #region Battery

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

                // Low
                case 2:
                    Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Low.png", UriKind.Relative));
                    break;

                // Medium
                case 3:
                    Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Medium.png", UriKind.Relative));
                    break;

                case 4:
                    Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-Medium.png", UriKind.Relative));
                    break;

                // High
                case 5:
                    Battery.Source = new BitmapImage(new Uri("Assets/Images/Battery/Battery-High.png", UriKind.Relative));
                    break;

                default:
                    break;
            }
        }

        #endregion Battery

        #region Wireless Signal

        /// <summary>
        /// Set the signal capacity based on the strength
        /// </summary>
        /// <param name="strength">The strength taking values: NO_SIGNAL, BAD_SIGNAL, GOOD_SIGNAL </param>
        private void UpdateSignalStrengthIcon(string strength)
        {
            switch (strength)
            {
                case "NO_SIGNAL":
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - No Signal.png", UriKind.Relative));
                    break;
                case "BAD_SIGNAL":
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 2.png", UriKind.Relative));
                    break;
                case "GOOD_SIGNAL":
                    Signal.Source = new BitmapImage(new Uri("Assets/Images/Signal/Wireless - Signal 4.png", UriKind.Relative));
                    break;
                default:
                    break;
            }
        }

        #endregion Wireless Signal


        #endregion StatusBar

        #region Settings

        #region General Settings

        #region Emotiv Toggle Switch

        // Emotiv Turned On
        private void EmotivToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // Try to connect the Emotiv device.

            // Connect Emotiv
            engine.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Connect();
            gyrotimer.Start();

            EmotivStatusLbl.Content = "Connected"; // Set to Connected
            UpdateBatteryCapacityIcon(BatteryLevel); // Get Battery Level
            UpdateSignalStrengthIcon(SignalStatus); // Get Wireless Signal Strength
        }

        // Emotiv Turned Off
        private void EmotivToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disconnect Emotiv
            engine.EmoStateUpdated -= new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            engine.UserAdded -= new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Disconnect();
            gyrotimer.Stop();

            EmotivStatusLbl.Content = "Not Connected"; // Set to Disconnected
            UpdateBatteryCapacityIcon(0); // Set Battery Level
            UpdateSignalStrengthIcon("NO_SIGNAL"); // Set Wireless Signal Strength
        }

        #endregion Emotiv Toggle Switch

        #region Get Running Time

        private string ConvertToTime(float GetTimeFromStartFloat)
        {
            int GetTimeFromStart = (Int32)GetTimeFromStartFloat;

            string hours = (GetTimeFromStart / 3600).ToString();
            string minutes = ((GetTimeFromStart / 60) % 60).ToString();
            string seconds = (GetTimeFromStart % 60).ToString();
            return (hours + " h " + minutes + " ' " + seconds + " \" ");
        }

        #endregion Get Running Time

        /// <summary>
        /// Sets the contact quality color for the contact quality provided.
        /// </summary>
        /// <param name="contactQuality"></param>
        /// <returns></returns>
        private Brush getContactQualityColor(string contactQuality)
        {
            switch (contactQuality)
            {
                case "EEG_CQ_NO_SIGNAL":
                    return Brushes.Black;
                case "EEG_CQ_VERY_BAD":
                    return Brushes.Red;
                case "EEG_CQ_POOR":
                    return Brushes.Orange;
                case "EEG_CQ_FAIR":
                    return Brushes.Yellow;
                case "EEG_CQ_GOOD":
                    return Brushes.Green;
                default:
                    return Brushes.Black;
            }
        }

        /// <summary>
        /// Updates the Emotiv Sensor Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        private void UpdateEmotivSensorData(Dictionary<EdkDll.EE_DataChannel_t, double[]> data, int i)
        {
            // EEG Sensor Data
            AF3 = data[EdkDll.EE_DataChannel_t.AF3][i];
            AF4 = data[EdkDll.EE_DataChannel_t.AF4][i];
            F3 = data[EdkDll.EE_DataChannel_t.F3][i];
            F4 = data[EdkDll.EE_DataChannel_t.F4][i];
            F7 = data[EdkDll.EE_DataChannel_t.F7][i];
            F8 = data[EdkDll.EE_DataChannel_t.F8][i];
            FC5 = data[EdkDll.EE_DataChannel_t.FC5][i];
            FC6 = data[EdkDll.EE_DataChannel_t.FC6][i];
            O1 = data[EdkDll.EE_DataChannel_t.O1][i];
            O2 = data[EdkDll.EE_DataChannel_t.O2][i];
            P7 = data[EdkDll.EE_DataChannel_t.P7][i];
            P8 = data[EdkDll.EE_DataChannel_t.P8][i];
            T7 = data[EdkDll.EE_DataChannel_t.T7][i];
            T8 = data[EdkDll.EE_DataChannel_t.T8][i];

            // Gyro Data
            GYROX = data[EdkDll.EE_DataChannel_t.GYROX][i];
            GYROY = data[EdkDll.EE_DataChannel_t.GYROY][i];

            // Other Data
            COUNTER = data[EdkDll.EE_DataChannel_t.COUNTER][i];
            ES_TIMESTAMP = data[EdkDll.EE_DataChannel_t.ES_TIMESTAMP][i];
            FUNC_ID = data[EdkDll.EE_DataChannel_t.FUNC_ID][i];
            FUNC_ID = data[EdkDll.EE_DataChannel_t.FUNC_VALUE][i];
            INTERPOLATED = data[EdkDll.EE_DataChannel_t.INTERPOLATED][i];
            MARKER = data[EdkDll.EE_DataChannel_t.MARKER][i];
            RAW_CQ = data[EdkDll.EE_DataChannel_t.RAW_CQ][i];
            SYNC_SIGNAL = data[EdkDll.EE_DataChannel_t.SYNC_SIGNAL][i];
            TIMESTAMP = data[EdkDll.EE_DataChannel_t.TIMESTAMP][i];
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

        #endregion General Settings

        #region Specific Settings

        #endregion Specific Settings

        #endregion Settings






    }
}
