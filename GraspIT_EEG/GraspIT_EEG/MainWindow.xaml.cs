/*
 * GraspIT EEG - Emotiv Project
 * 
 * Columbia Robotics Lab
 * Columbia University Copyright ©  2013
 * 
 * Supervisor: Professor Peter Allen
 * Coordination: Jon Weisz
 * Development: Alexandros Sigaras
 * Email: alex@sigaras.com
 * 
 * Description
 * Using SSVEP Signals to control a robotic arm by thought.
 * 
 */

#region Libraries

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
using System.IO.Ports;

// Custom written libraries
using GraspIT_EEG.Model;
using GraspIT_EEG.DialogBoxes;

// Metro WPF Library - MahApps
using MahApps.Metro.Controls;

// Realtime Charts - Telerik RadControls for WPF
using Telerik.Windows.Controls.ChartView;

// Emotiv Library
using Emotiv;
using System.IO;
using GraspIT_EEG.Properties;

#endregion Libraries

namespace GraspIT_EEG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region Declarations

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

        // Display a the help file on how to use the application.
        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            string helpFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Help\\help.pdf";
            System.Diagnostics.Process.Start(helpFilePath);
        }

        #endregion UI Specific

        #region Emotiv

        #region Device
        EmoEngine engine = EmoEngine.Instance;
        EmoState es;

        bool emotivIsConnected = false;

        DispatcherTimer emotivDataCollectionTimer = new DispatcherTimer();

        // Emotiv Wireless Signal
        public string SignalStatus = "NO_SIGNAL";

        // Emotiv Battery
        public int BatteryLevel = 0;
        public int MaxBatteryLevel = 5;

        // Others
        public double COUNTER, ES_TIMESTAMP, FUNC_ID, FUNC_VALUE, INTERPOLATED, MARKER, RAW_CQ, SYNC_SIGNAL, TIMESTAMP;

        float bufferSize;
        uint samplingRate;
        float elapsed;

        #endregion Device

        #region Expressiv (EMG Signals)

        #endregion Expressiv (EMG Signals)

        #region Affectiv

        // EdkDll.EE_AffectivAlgo_t  must declare algo type to finish affectiv.

        bool AffectivIsActive;
        float AffectiveEngagementBoredom;
        float AffectivMeditation;
        float AffectivFrustration;
        float AffectivExcitementShort;
        float AffectivExcitementLong;

        List<AffectivChartDataObject> AffectivEngagementBoredomList = new List<AffectivChartDataObject>();
        List<AffectivChartDataObject> AffectivMeditationList = new List<AffectivChartDataObject>();
        List<AffectivChartDataObject> AffectivFrustrationList = new List<AffectivChartDataObject>();
        List<AffectivChartDataObject> AffectivExcitementShortList = new List<AffectivChartDataObject>();
        List<AffectivChartDataObject> AffectivExcitementLongList = new List<AffectivChartDataObject>();

        #endregion Affectiv

        #region Cognitiv

        #endregion Cognitiv

        #region EEG

        public double AF3, F7, F3, FC5, T7, P7, O1, O2, P8, T8, FC6, F4, F8, AF4;

        List<EEGChartDataObject> AF3List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> F7List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> F3List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> FC5List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> T7List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> P7List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> O1List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> O2List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> P8List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> T8List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> FC6List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> F4List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> F8List = new List<EEGChartDataObject>();
        List<EEGChartDataObject> AF4List = new List<EEGChartDataObject>();

        #endregion EEG

        #region Gyro

        public double GYROX, GYROY;
        int xmax = 0, ymax = 0;

        List<GyroChartDataObject> GyroXList = new List<GyroChartDataObject>();
        List<GyroChartDataObject> GyroYList = new List<GyroChartDataObject>();

        #endregion Gyro

        #region Sequence Number

        List<SequenceNumberChartDataObject> SequenceNumberList = new List<SequenceNumberChartDataObject>();

        #endregion Sequence Number

        #region Packet Loss

        List<PacketLossChartDataObject> PacketLossList = new List<PacketLossChartDataObject>();

        #endregion Packet Loss

        #region User

        // .emu User Profile Location
        public string[] emuFilePaths = Directory.GetFiles("C:\\ProgramData\\Emotiv\\");

        // User ID
        uint userID = (uint)0;
        //int seconds = 0;
        int x = 0;
        int y = 0;

        #endregion User

        #endregion Emotiv

        #region SSVEP

        #region SSVEP Timers Declaration

        DispatcherTimer SSVEPFlashDuration = new DispatcherTimer();
        DispatcherTimer SSVEPNoFlashDuration = new DispatcherTimer();
        DispatcherTimer SSVEPFlashingPeriod = new DispatcherTimer();

        #endregion SSVEP Timers Declaration

        double gain = 1.22698672;
        double[] coefficients = new double[6] { 0.6642317127, 0.2500608525, -2.2141423193, -0.6015694459, 2.5249625592, 0.3764534782 };
        
        #region Butterworth Filters Declaration

        Butterworth ButAF3 = new Butterworth();
        Butterworth ButF7 = new Butterworth();
        Butterworth ButF3 = new Butterworth();
        Butterworth ButFC5 = new Butterworth();
        Butterworth ButT7 = new Butterworth();
        Butterworth ButP7 = new Butterworth();
        Butterworth ButO1 = new Butterworth();
        Butterworth ButO2 = new Butterworth();
        Butterworth ButP8 = new Butterworth();
        Butterworth ButT8 = new Butterworth();
        Butterworth ButFC6 = new Butterworth();
        Butterworth ButF4 = new Butterworth();
        Butterworth ButF8 = new Butterworth();
        Butterworth ButAF4 = new Butterworth();
        double butAF3, butF7, butF3, butFC5, butT7, butP7, butO1, butO2, butP8, butT8, butFC6, butF4, butF8, butAF4;

        #endregion Butterworth Filters Declaration

        #endregion SSVEP

        #region R2D2

        private static readonly string R2D2ComPort = Settings.Default.R2D2ComPort;

        #endregion R2D2

        #endregion Declarations

        public MainWindow()
        {
            InitializeComponent();
            //engine.EmoStateUpdated +=engine_EmoStateUpdated;
            LoadUsers();
            #region Instantiate Timers

            emotivDataCollectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            emotivDataCollectionTimer.Tick += emotivDataCollectionTimer_Tick;

            SSVEPFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 125);
            SSVEPFlashDuration.Tick += SSVEPFlashDuration_Tick;

            SSVEPNoFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 475);
            SSVEPNoFlashDuration.Tick += SSVEPNoFlashDuration_Tick;

            SSVEPFlashingPeriod.Interval = new TimeSpan(0, 0, 0, 0, 8);
            SSVEPFlashingPeriod.Tick += SSVEPFlashingPeriod_Tick;

            #endregion Instantiate Timers
        }

        #region TitleBar

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

        #endregion TitleBar

        #region Settings

        #region General Settings

        #region Emotiv Toggle Switch

        /// <summary>
        /// Emotiv Turned On
        /// </summary>
        private void EmotivToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // Try to connect the Emotiv device.

            // Connect Emotiv
            engine.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Connect();
            emotivDataCollectionTimer.Start();

            EmotivStatusLbl.Content = "Connected";      // Set to Connected
            emotivIsConnected = true;
            LoadUserProfile();
            UpdateBatteryCapacityIcon(BatteryLevel);    // Get Battery Level
            UpdateSignalStrengthIcon(SignalStatus);     // Get Wireless Signal Strength

            x = y = 0;                                  // Reset Gyros
        }

        /// <summary>
        /// Emotiv Turned Off
        /// <summary>
        private void EmotivToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disconnect Emotiv
            engine.EmoStateUpdated -= new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
            engine.UserAdded -= new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            engine.Disconnect();
            emotivDataCollectionTimer.Stop();

            EmotivStatusLbl.Content = "Not Connected";      // Set to Disconnected
            emotivIsConnected = false;
            UpdateBatteryCapacityIcon(0);                   // Set Battery Level
            UpdateSignalStrengthIcon("NO_SIGNAL");          // Set Wireless Signal Strength

            SamplingRate.Content = "No Data";
            BufferSize.Content = "No Data";

            // Turn off the color of the contacts to Black.
            AF3Contact.Fill = Brushes.Black;
            AF4Contact.Fill = Brushes.Black;
            F7Contact.Fill = Brushes.Black;
            F3Contact.Fill = Brushes.Black;
            F4Contact.Fill = Brushes.Black;
            F8Contact.Fill = Brushes.Black;
            FC5Contact.Fill = Brushes.Black;
            FC6Contact.Fill = Brushes.Black;
            T7Contact.Fill = Brushes.Black;
            T8Contact.Fill = Brushes.Black;
            CMSContact.Fill = Brushes.Black;
            DRLContact.Fill = Brushes.Black;
            P7Contact.Fill = Brushes.Black;
            P8Contact.Fill = Brushes.Black;
            O1Contact.Fill = Brushes.Black;
            O2Contact.Fill = Brushes.Black;
        }

        #endregion Emotiv Toggle Switch

        #region Get Running Time

        /// <summary>
        /// Convert Elapsed seconds to a string format to display.
        /// </summary>
        /// <param name="GetTimeFromStartFloat"></param>
        /// <returns></returns>
        private string ConvertToTime(float GetTimeFromStartFloat)
        {
            int GetTimeFromStart = (Int32)GetTimeFromStartFloat;

            string hours = (GetTimeFromStart / 3600).ToString();
            string minutes = ((GetTimeFromStart / 60) % 60).ToString();
            string seconds = (GetTimeFromStart % 60).ToString();
            return (hours + " h " + minutes + " ' " + seconds + " \" ");
        }

        #endregion Get Running Time

        #region Get Contact Quality Color

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

        #endregion Get Contact Quality Color

        #region Update Emotiv Sensor Data

        /// <summary>
        /// Updates the Emotiv Sensor Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="i"></param>
        private void UpdateEmotivSensorData(Dictionary<EdkDll.EE_DataChannel_t, double[]> data, int i)
        {
            #region EEG Sensor Data

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

            #endregion EEG Sensor Data

            #region Butterworth Filtered Data

            // Butterworth Filtered Data
            butAF3 = ButAF3.getFilteredValue(AF3, coefficients, gain);
            butAF4 = ButAF4.getFilteredValue(AF4, coefficients, gain);
            butF3 = ButF3.getFilteredValue(F3, coefficients, gain);
            butF4 = ButF4.getFilteredValue(F4, coefficients, gain);
            butF7 = ButF7.getFilteredValue(F7, coefficients, gain);
            butF8 = ButF8.getFilteredValue(F8, coefficients, gain);
            butFC5 = ButFC5.getFilteredValue(FC5, coefficients, gain);
            butFC6 = ButFC6.getFilteredValue(FC6, coefficients, gain);
            butO1 = ButO1.getFilteredValue(O1, coefficients, gain);
            butO2 = ButO2.getFilteredValue(O2, coefficients, gain);
            butP7 = ButP7.getFilteredValue(P7, coefficients, gain);
            butP8 = ButP8.getFilteredValue(P8, coefficients, gain);
            butT7 = ButT7.getFilteredValue(T7, coefficients, gain);
            butT8 = ButT8.getFilteredValue(T8, coefficients, gain);

            #endregion Butterworth Filtered Data

            // Set Values            
            AF4val.Content = ((Int32)AF4).ToString();
            AF4butval.Content = ((Int32)butAF4).ToString();

            #region Gyro Data

            // Gyro Data
            GYROX = data[EdkDll.EE_DataChannel_t.GYROX][i];
            GYROY = data[EdkDll.EE_DataChannel_t.GYROY][i];

            #endregion Gyro Data

            #region Other Data

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

            #endregion Other Data

            InterpolatedLbl.Content = SYNC_SIGNAL.ToString();

        }

        #endregion Update Emotiv Sensor Data

        // Add User.
        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (emotivIsConnected)
            {
                string newUserName = "";
                AddUser dialog = new AddUser();
                dialog.ShowDialog();
                if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                {
                    // Get User Name
                    newUserName = dialog.UserNameTxtBox.Text;
                }
                string newUserNamePath = "C:\\ProgramData\\Emotiv\\" + newUserName + ".emu";

                // User gave a name
                if (newUserName != "")
                {
                    // Name doesn't exist already
                    if (!emuFilePaths.Contains(newUserNamePath))
                    {
                        // Save new user profile
                        engine.EE_SaveUserProfile(userID, newUserNamePath);

                        // Load profile
                        engine.LoadUserProfile(userID, newUserNamePath);
                    }
                    else
                    {
                        MessageBox.Show("User already exists!");
                    }
                }
            }
            else
            {
                MessageBox.Show("Device has to be connected in order to add a user.");
            }
        }

        // Remove User.
        private void RemoveUserBtn_Click(object sender, RoutedEventArgs e)
        {
            RemoveUser dialog = new RemoveUser();
            dialog.ShowDialog();

            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            { 
                // Remove user
                File.Delete(emuFilePaths[UsersComboBox.SelectedIndex]);
            }
            LoadUsers();
        }

        // Save User.
        private void SaveUserBtn_Click(object sender, RoutedEventArgs e)
        {
            if (emotivIsConnected)
            {
                SaveUserProfile();
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Emotiv has to be connected to save user");
            }
        }

        private void SaveUserProfile()
        {
            engine.EE_SaveUserProfile(userID, "C:\\ProgramData\\Emotiv\\" + System.IO.Path.GetFileName(emuFilePaths[UsersComboBox.SelectedIndex]));
        }

        // Train User.
        private void TrainUserBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        private void LoadUserProfile()
        {
            engine.LoadUserProfile(userID, "C:\\ProgramData\\Emotiv\\" + System.IO.Path.GetFileName(emuFilePaths[UsersComboBox.SelectedIndex]));
        }

        #region Emotiv Event Handlers

        void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
        {
            es = e.emoState;
            elapsed = es.GetTimeFromStart();

            SignalStatus = es.GetWirelessSignalStatus().ToString();
            Uptime.Content = ConvertToTime(es.GetTimeFromStart());
            es.GetBatteryChargeLevel(out BatteryLevel, out MaxBatteryLevel);
            UpdateSensorContactQuality(es);

            #region Expressiv

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
            if (es.ExpressivIsEyesOpen())
            {
                MiddleFaceAction.Content = "Eyes Open";
            }
            else
            {
                MiddleFaceAction.Content = "Eyes Closed";
            }
            expressivIsLeftWink = es.ExpressivIsLeftWink();
            expressivIsLookingDown = es.ExpressivIsLookingDown();
            expressivIsLookingLeft = es.ExpressivIsLookingLeft();
            expressivIsLookingRight = es.ExpressivIsLookingRight();
            expressivIsLookingUp = es.ExpressivIsLookingUp();
            expressivIsRightWink = es.ExpressivIsRightWink();

            
            //es.ExpressivGetEyeLocation(out eyeXCoordinate, out eyeYCoordinate);
            //EyeXCoordinate.Content = eyeXCoordinate.ToString();
            //EyeYCoordinate.Content = eyeYCoordinate.ToString();

            ClenchCheckBox.IsChecked = false;
            EyebrowsCheckBox.IsChecked = false;
            SmileCheckBox.IsChecked = false;

            Eyebrows.Content = es.ExpressivGetEyebrowExtent().ToString();
            Smile.Content = es.ExpressivGetSmileExtent().ToString();
            LowerFaceAction.Content = lowerFaceAction.ToString();
            UpperFaceAction.Content = upperFaceAction.ToString();

            #region R2D2

            if (R2D2.isConnected)
            {
                if(lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_CLENCH)
                {
                   R2D2.MoveForward();
                }
                else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_LEFT)
                {
                   R2D2.MoveLeft();
                }
                else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_RIGHT)
                {
                   R2D2.MoveRight();
                }
                else if (es.ExpressivGetEyebrowExtent() > 0.10)
                {
                    R2D2.Stop();
                }
            }

            #endregion R2D2

            if (es.ExpressivGetEyebrowExtent() > 0.10)
            {
                EyebrowRect.Fill = Brushes.Green;
                EyebrowsCheckBox.IsChecked = true;
            }
            else
            {
                EyebrowRect.Fill = Brushes.Red;
                EyebrowsCheckBox.IsChecked = false;
            }

            Clench.Content = es.ExpressivGetClenchExtent().ToString();
            if (es.ExpressivGetClenchExtent() > 0.10)
            {
                ClenchRect.Fill = Brushes.Green;
                ClenchCheckBox.IsChecked = true;
            }
            else
            {
                ClenchRect.Fill = Brushes.Red;
                ClenchCheckBox.IsChecked = false;
            }

            if (es.ExpressivGetSmileExtent() > 0.05)
            {
                SmileRect.Fill = Brushes.Green;
                SmileCheckBox.IsChecked = true;
            }
            else
            {
                SmileRect.Fill = Brushes.Red;
                SmileCheckBox.IsChecked = false;
            }

            SamplingRate.Content = samplingRate.ToString();
            BufferSize.Content = bufferSize.ToString();

            #endregion Expressiv

            #region Affectiv

            //AffectivIsActive = es.AffectivIsActive; Missing the algo type
            AffectiveEngagementBoredom = es.AffectivGetEngagementBoredomScore();    // Get Engagement/Boredom Score.
            AffectivExcitementLong = es.AffectivGetExcitementLongTermScore();       // Get Excitement Long Term Score.
            AffectivExcitementShort = es.AffectivGetExcitementShortTermScore();     // Get Excitement Short Term Score.
            AffectivFrustration = es.AffectivGetFrustrationScore();                 // Get Frustration Score.
            AffectivMeditation = es.AffectivGetMeditationScore();              // Get Meditation Score.

            #endregion Affectiv

            #region Cognitiv

            EdkDll.EE_CognitivAction_t EEGAction;
            EEGAction = es.CognitivGetCurrentAction();
            double cognitivpower = es.CognitivGetCurrentActionPower();
            cognitivPower.Content = cognitivpower.ToString();
            bool cognitivIsNoisy;
            cognitivIsNoisy = es.CognitivIsActive();
            if (cognitivIsNoisy)
            {
                cognitivIsActive.Content = "Noisy";
            }
            else
            {
                cognitivIsActive.Content = "Ok";
            }

            // Get Cognitiv Action
            switch (EEGAction)
            {
                case EdkDll.EE_CognitivAction_t.COG_DISAPPEAR:
                    cognitivIsState.Content = "Dissapear";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_DROP:
                    MessageBox.Show("Drop");
                    cognitivIsState.Content = "Drop";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_LEFT:
                    cognitivIsState.Content = "Left";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_LIFT:
                    cognitivIsState.Content = "Lift";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_NEUTRAL:
                    cognitivIsState.Content = "Neutral";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_PULL:
                    cognitivIsState.Content = "Pull";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_PUSH:
                    cognitivIsState.Content = "Push";
                    if (cognitivpower > 0.2)
                    {
                        R2D2.MoveBack();
                    }
                    break;
                case EdkDll.EE_CognitivAction_t.COG_RIGHT:
                    cognitivIsState.Content = "Right";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_CLOCKWISE:
                    cognitivIsState.Content = "Rotate Clockwise";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_COUNTER_CLOCKWISE:
                    cognitivIsState.Content = "Rotate Counter Clockwise";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_FORWARDS:
                    cognitivIsState.Content = "Rotate Forwards";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_LEFT:
                    cognitivIsState.Content = "Rotate Left";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_REVERSE:
                    cognitivIsState.Content = "Rotate Reverse";
                    break;
                case EdkDll.EE_CognitivAction_t.COG_ROTATE_RIGHT:
                    cognitivIsState.Content = "Rotate Right";
                    break;
                default:
                    break;
            }

            #endregion Cognitiv

        }

        /// <summary>
        /// Update Sensor Data Contact Quality
        /// </summary>
        /// <param name="es">EmoState</param>
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

        #endregion General Settings

        #region Specific Settings

        void emotivDataCollectionTimer_Tick(object sender, EventArgs e)
        {
            engine.ProcessEvents();

            if ((int)userID == -1)
                return;

            UpdateBatteryCapacityIcon(BatteryLevel);    // Get Battery Level
            UpdateSignalStrengthIcon(SignalStatus);     // Get Wireless Signal Strength
            
            try
            {
                x = y = 0;
                engine.HeadsetGetGyroDelta(userID, out x, out y);
                GyroStatus.Content = "On";

                xmax += x;
                ymax += y;
                xValue.Content = xmax.ToString();
                xValueMax.Content = x.ToString();
                yValue.Content = ymax.ToString();
                yValueMax.Content = y.ToString();
                xValueMax_Copy.Content = GYROX.ToString();
                yValueMax_Copy.Content = GYROY.ToString();
            }
            catch (Exception)
            {
                GyroStatus.Content = "Calibrating...";
            }

            UpdateGraphs();

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData(userID);

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

        /// <summary>
        /// Update Graphs
        /// </summary>
        private void UpdateGraphs()
        {

            #region EEG Graphs

            #region O1Graph

            EEGChartDataObject O1Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = AF4
            };
            O1List.Add(O1Obj);

            LineSeries O1Series = (LineSeries)this.O1Chart.Series[0];
            O1Series.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            O1Series.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            O1Series.ItemsSource = O1List;

            #endregion O1Graph

            #region O2Graph

            EEGChartDataObject O2Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = butAF4
            };

            O2List.Add(O2Obj);

            LineSeries O2Series = (LineSeries)this.O2Chart.Series[0];
            O2Series.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            O2Series.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            O2Series.ItemsSource = O2List;

            #endregion O2Graph

            #endregion EEG Graphs

            #region Expressiv Graphs

            #region Engagement/Boredom Graph

            AffectivChartDataObject AffectivEngagementBoredomObj = new AffectivChartDataObject
            {
                Time = DateTime.Now,
                Value = AffectiveEngagementBoredom
            };
            AffectivEngagementBoredomList.Add(AffectivEngagementBoredomObj);

            LineSeries AffectivEngagementBoredomSeries = (LineSeries)this.AffectivChart.Series[0];
            AffectivEngagementBoredomSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AffectivEngagementBoredomSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AffectivEngagementBoredomSeries.ItemsSource = AffectivEngagementBoredomList;

            #endregion Engagement/Boredom Graph

            #region Meditation Graph

            AffectivChartDataObject AffectivMeditationObj = new AffectivChartDataObject
            {
                Time = DateTime.Now,
                Value = AffectivMeditation
            };
            AffectivMeditationList.Add(AffectivMeditationObj);

            LineSeries AffectivMeditationSeries = (LineSeries)this.AffectivChart.Series[1];
            AffectivMeditationSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AffectivMeditationSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AffectivMeditationSeries.ItemsSource = AffectivMeditationList;

            #endregion Meditation Graph

            #region Frustration Graph

            AffectivChartDataObject AffectivFrustrationObj = new AffectivChartDataObject
            {
                Time = DateTime.Now,
                Value = AffectivFrustration
            };
            AffectivFrustrationList.Add(AffectivFrustrationObj);

            LineSeries AffectivFrustrationSeries = (LineSeries)this.AffectivChart.Series[2];
            AffectivFrustrationSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AffectivFrustrationSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AffectivFrustrationSeries.ItemsSource = AffectivFrustrationList;

            #endregion Frustration Graph

            #region Short Excitement Graph

            AffectivChartDataObject AffectivExcitementShortObj = new AffectivChartDataObject
            {
                Time = DateTime.Now,
                Value = AffectivExcitementShort
            };
            AffectivExcitementShortList.Add(AffectivExcitementShortObj);

            LineSeries AffectivExcitementShortSeries = (LineSeries)this.AffectivChart.Series[3];
            AffectivExcitementShortSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AffectivExcitementShortSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AffectivExcitementShortSeries.ItemsSource = AffectivExcitementShortList;

            #endregion Short Excitement Graph

            #region Long Excitement Graph

            AffectivChartDataObject AffectivExcitementLongObj = new AffectivChartDataObject
            {
                Time = DateTime.Now,
                Value = AffectivExcitementLong
            };
            AffectivExcitementLongList.Add(AffectivExcitementLongObj);

            LineSeries AffectivExcitementLongSeries = (LineSeries)this.AffectivChart.Series[4];
            AffectivExcitementLongSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AffectivExcitementLongSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AffectivExcitementLongSeries.ItemsSource = AffectivExcitementLongList;

            #endregion Long Excitement Graph

            #endregion Expressiv Graphs

            #region Gyro Graphs

            #region GyroXGraph

            GyroChartDataObject GyroXObj = new GyroChartDataObject
            {
                //Time = ConvertToTime(elapsed),
                Time = DateTime.Now,
                Value = x
            };
            GyroXList.Add(GyroXObj);

            LineSeries xSeries = (LineSeries)this.GyroXChart.Series[0];
            xSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            xSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            xSeries.ItemsSource = GyroXList;

            #endregion GyroXGraph

            #region GyroYGraph

            GyroChartDataObject GyroYObj = new GyroChartDataObject
            {
                //Time = ConvertToTime(elapsed),
                Time = DateTime.Now,
                Value = y
            };
            GyroYList.Add(GyroYObj);

            LineSeries ySeries = (LineSeries)this.GyroYChart.Series[0];
            ySeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            ySeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            ySeries.ItemsSource = GyroYList;

            #endregion GyroYGraph

            #endregion Gyro Graphs

            #region Data Graphs

            #region SequenceNumberGraph

            SequenceNumberChartDataObject SequenceNumberObj = new SequenceNumberChartDataObject
            {
                //Time = ConvertToTime(elapsed),
                Time = DateTime.Now,
                Value = COUNTER
            };
            SequenceNumberList.Add(SequenceNumberObj);

            LineSeries SequenceNumberSeries = (LineSeries)this.SequenceNumberChart.Series[0];
            SequenceNumberSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            SequenceNumberSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            SequenceNumberSeries.ItemsSource = SequenceNumberList;

            #endregion SequenceNumberGraph

            #region PacketLossGraph

            PacketLossChartDataObject PacketLossObj = new PacketLossChartDataObject
            {
                //Time = ConvertToTime(elapsed),
                Time = DateTime.Now,
                Value = RAW_CQ
            };
            PacketLossList.Add(PacketLossObj);

            LineSeries PacketLossSeries = (LineSeries)this.PacketLossChart.Series[0];
            PacketLossSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            PacketLossSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            PacketLossSeries.ItemsSource = PacketLossList;

            #endregion PacketLossGraph

            #endregion Data Graphs
        }

        #endregion Specific Settings

        #endregion Settings

        #region SSVEP

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

        private void R2D2Conenctbtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.ConnectNXT(R2D2ComPort);
            R2D2.showCULogo();
        }

        #endregion SSVEP

        private void LoadUsers()
        {
            UsersComboBox.Items.Clear();
            emuFilePaths = Directory.GetFiles("C:\\ProgramData\\Emotiv\\");
            foreach (var user in emuFilePaths)
            {
                UsersComboBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(user));   
            }

            UsersComboBox.SelectedIndex = 0;
        }

        private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (emotivIsConnected)
            {
                LoadUserProfile();
            }


      
        }

        #region TestCode

        //double gain = 1.22698672;
        //double[] coefficients = new double[6] { 0.6642317127, 0.2500608525, -2.2141423193, -0.6015694459, 2.5249625592, 0.3764534782};
        
        #endregion

    }
}