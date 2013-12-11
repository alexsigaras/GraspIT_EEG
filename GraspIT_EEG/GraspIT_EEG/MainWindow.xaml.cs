/*
 * GraspIT EEG - Emotiv Project
 * 
 * Columbia Robotics Lab
 * Columbia University Copyright ©  2013
 * 
 * Supervisor: Professor Peter Allen
 * Development: Alexandros Sigaras
 * Email: alex@sigaras.com
 * 
 * Description:
 * Using Emotiv EEG neuroheadset to read EEG & EMG brain signals to control the following robots:
 * - iRobot Create
 * - OWI 535 Robotic Arm
 * - Lego Mindstorms NXT (R2D2)
 * - Talos (Pending)
 * - Staubli Arm - GraspIT (Pending)
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
using GraspIT_EEG.Model.Robots;
using GraspIT_EEG.DialogBoxes;

// Metro WPF Library - MahApps
using MahApps.Metro.Controls;

// Realtime Charts - Telerik RadControls for WPF
using Telerik.Windows.Controls.ChartView;

// Emotiv Library
using Emotiv;
using System.IO;
using GraspIT_EEG.Properties;

// Webcam Library
using MjpegProcessor;

#endregion Libraries

namespace GraspIT_EEG
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		#region Declarations

        MjpegDecoder _mjpeg;
        public int Hz;
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);

		#region UI Specific

		#region Colors

		public Color columbiaBlue = Color.FromArgb(204, 142, 180, 227);
		public Color royalBlue = Color.FromArgb(204, 64, 106, 165);
		// Check for redundancy
		public SolidColorBrush bgColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
		public SolidColorBrush flashColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
		public SolidColorBrush currentColor = new SolidColorBrush();

		#endregion Colors

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
					Storyboard SettingsUnload = (Storyboard)FindResource("SettingsPageOut");
					SettingsUnload.Begin(this);
					break;
				case 3:
					Storyboard TrainingUnload = (Storyboard)FindResource("TrainingPageOut");
					TrainingUnload.Begin(this);
					break;
				case 4:
					Storyboard SSVEPTrainingUnload = (Storyboard)FindResource("SSVEPTrainingPageOut");
					SSVEPTrainingUnload.Begin(this);
					break;
				case 5:
					Storyboard CognitivTrainingUnload = (Storyboard)FindResource("CognitivTrainingPageOut");
					CognitivTrainingUnload.Begin(this);
					break;
				case 6:
					Storyboard SSVEPUnload = (Storyboard)FindResource("SSVEPPageOut");
					SSVEPUnload.Begin(this);
					break;
				case 7:
					Storyboard CognitivUnload = (Storyboard)FindResource("CognitivPageOut");
					CognitivUnload.Begin(this);
					break;
				case 8:
					Storyboard EMGUnload = (Storyboard)FindResource("EMGPageOut");
					EMGUnload.Begin(this);
					break;
				default:
					break;
			}
		}

		#endregion Animation Function

		#region Navigation Functions

        int currentPage = 1;

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
    
		#endregion UI Specific

		#region R2D2

		private static readonly string R2D2ComPort = Settings.Default.R2D2ComPort;

		#endregion R2D2

		#region Emotiv

		#region Device
		EmoEngine engine = EmoEngine.Instance;
		public EmoState es;

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

		float AffectiveEngagementBoredom;
		float AffectivMeditation;
		float AffectivFrustration;
		float AffectivExcitementShort;
		float AffectivExcitementLong;

		// Affectiv Lists
		List<AffectivChartDataObject> AffectivEngagementBoredomList = new List<AffectivChartDataObject>();
		List<AffectivChartDataObject> AffectivMeditationList = new List<AffectivChartDataObject>();
		List<AffectivChartDataObject> AffectivFrustrationList = new List<AffectivChartDataObject>();
		List<AffectivChartDataObject> AffectivExcitementShortList = new List<AffectivChartDataObject>();
		List<AffectivChartDataObject> AffectivExcitementLongList = new List<AffectivChartDataObject>();

		#endregion Affectiv

		#region Cognitiv (EEG Signals)

        #endregion Cognitiv (EEG Signals)

        #region EEG

        public double AF3, F7, F3, FC5, T7, P7, O1, O2, P8, T8, FC6, F4, F8, AF4;

		// EEG Lists
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


        List<EEGChartDataObject> butAF4List = new List<EEGChartDataObject>();

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

		#region Raw CQ Signal Amplitude

		List<RawCQSignalAmplitudeChartDataObject> RawCQSignalAmplitudeList = new List<RawCQSignalAmplitudeChartDataObject>();

        #endregion Raw CQ Signal Amplitude

        #region User

        // .emu User Profile Location
		private static readonly string emotivFilePath = Settings.Default.EmotivUserFilePath;
		public string[] emuFilePaths = Directory.GetFiles(emotivFilePath);
		// User ID
		uint userID = (uint)0;
		//int seconds = 0;
		int x = 0;
		int y = 0;

		#endregion User

		#endregion Emotiv

        //#region SSVEP

        //#region SSVEP Timers Declaration

        //DispatcherTimer SSVEPFlashDuration = new DispatcherTimer();
        //DispatcherTimer SSVEPNoFlashDuration = new DispatcherTimer();
        //DispatcherTimer SSVEPFlashingPeriod = new DispatcherTimer();

        //#endregion SSVEP Timers Declaration

		public static double[] gain = new double[] { Settings.Default.Bandpass6_7gain, Settings.Default.Bandpass7_8gain, Settings.Default.Bandpass8_9gain, Settings.Default.Bandpass9_11gain };
		public static double[][] coefficients = new double[][] {
																	new double[6] {Settings.Default.Bandpass6_7coef1, Settings.Default.Bandpass6_7coef2, Settings.Default.Bandpass6_7coef3, Settings.Default.Bandpass6_7coef4, Settings.Default.Bandpass6_7coef5, Settings.Default.Bandpass6_7coef6},
																	new double[6] {Settings.Default.Bandpass7_8coef1, Settings.Default.Bandpass7_8coef2, Settings.Default.Bandpass7_8coef3, Settings.Default.Bandpass7_8coef4, Settings.Default.Bandpass7_8coef5, Settings.Default.Bandpass7_8coef6},
																	new double[6] {Settings.Default.Bandpass8_9coef1, Settings.Default.Bandpass8_9coef2, Settings.Default.Bandpass8_9coef3, Settings.Default.Bandpass8_9coef4, Settings.Default.Bandpass8_9coef5, Settings.Default.Bandpass8_9coef6},
																	new double[6] {Settings.Default.Bandpass9_11coef1, Settings.Default.Bandpass9_11coef2, Settings.Default.Bandpass9_11coef3, Settings.Default.Bandpass9_11coef4, Settings.Default.Bandpass9_11coef5, Settings.Default.Bandpass9_11coef6}
																};
		
		#region Butterworth Filters Declaration

        Butterworth[] ButAF3 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButF7 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButF3 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButFC5 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButT7 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButP7 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButO1 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButO2 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButP8 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButT8 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButFC6 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButF4 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButF8 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
        Butterworth[] ButAF4 = new Butterworth[] { new Butterworth(), new Butterworth(), new Butterworth(), new Butterworth() };
		
        double [] butAF3 = new double[4];
        double [] butF7 = new double[4];
        double [] butF3 = new double[4];
        double [] butFC5 = new double[4];
        double [] butT7 = new double[4];
        double [] butP7 = new double[4];
        double [] butO1 = new double[4];
        double [] butO2 = new double[4];
        double [] butP8 = new double[4];
        double [] butT8 = new double[4];
        double [] butFC6 = new double[4];
        double [] butF4 = new double[4];
        double [] butF8 = new double[4];
        double [] butAF4 = new double[4];

		#endregion Butterworth Filters Declaration

        //#endregion SSVEP


		#endregion Declarations

		public MainWindow()
		{
			InitializeComponent();
            _mjpeg = new MjpegDecoder();
            _mjpeg.FrameReady += mjpeg_FrameReady;
			LoadUsers(); // Load the user Profiles

			#region Instantiate Timers

			emotivDataCollectionTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
			emotivDataCollectionTimer.Tick += emotivDataCollectionTimer_Tick;

			//SSVEPFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 125);
			//SSVEPFlashDuration.Tick += SSVEPFlashDuration_Tick;

			//SSVEPNoFlashDuration.Interval = new TimeSpan(0, 0, 0, 0, 475);
			//SSVEPNoFlashDuration.Tick += SSVEPNoFlashDuration_Tick;

			//SSVEPFlashingPeriod.Interval = new TimeSpan(0, 0, 0, 0, 8);
			//SSVEPFlashingPeriod.Tick += SSVEPFlashingPeriod_Tick;

			#endregion Instantiate Timers
		}

        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            Webcam.Source = e.BitmapImage;
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

        #region Display Help

        // Display a the help file on how to use the application.
        private void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            string helpFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Assets\\Help\\help.pdf";
            System.Diagnostics.Process.Start(helpFilePath);
        }

        #endregion Display Help

		#endregion TitleBar

		#region Main Menu

        #region Selections

        string methodSelected = "";
        string robotSelected = "";
        string trainingTileSelected = "";

        #endregion Selections

		#region 1st Column Tiles

		private void EMGBtn_Click(object sender, RoutedEventArgs e)
		{
			if ((EMGBtn.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
			{
				// Reset methodSelected Buttons.
				deselectMethodSelectedButtons();

				EMGBtn.Background = new SolidColorBrush(columbiaBlue);
				methodSelected = "EMG";
			}
			else // Tile Unchecked
			{
				EMGBtn.Background = new SolidColorBrush(royalBlue);
				methodSelected = "";
			}
		}

		private void EEGBtn_Click(object sender, RoutedEventArgs e)
		{
			if ((EEGBtn.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
			{
				// Reset methodSelected Buttons.
				deselectMethodSelectedButtons();

				EEGBtn.Background = new SolidColorBrush(columbiaBlue);
				methodSelected = "EEG";
			}
			else // Tile Unchecked
			{
				EEGBtn.Background = new SolidColorBrush(royalBlue);
				methodSelected = "";
			}
		}

		private void deselectMethodSelectedButtons()
		{
			EMGBtn.Background = new SolidColorBrush(royalBlue);
			EEGBtn.Background = new SolidColorBrush(royalBlue);
		}

		#endregion 1st Column Tiles

		#region 2nd Column Tiles

		private void R2D2Btn_Click(object sender, RoutedEventArgs e)
		{
			if ((R2D2Btn.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
			{
				// Reset methodSelected Buttons.
				deselectRobotSelectedButtons();

				R2D2Btn.Background = new SolidColorBrush(columbiaBlue);
				robotSelected = "R2D2";
			}
			else // Tile Unchecked
			{
				R2D2Btn.Background = new SolidColorBrush(royalBlue);
				robotSelected = "";
			}
		}

		private void OWIArmBtn_Click(object sender, RoutedEventArgs e)
		{
			if ((OWIArmBtn.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
			{
				// Reset methodSelected Buttons.
				deselectRobotSelectedButtons();

				OWIArmBtn.Background = new SolidColorBrush(columbiaBlue);
				robotSelected = "OWI535";
			}
			else // Tile Unchecked
			{
				OWIArmBtn.Background = new SolidColorBrush(royalBlue);
				robotSelected = "";
			}
		}

		private void iRobotCreateBtn_Click(object sender, RoutedEventArgs e)
		{
			if ((iRobotCreateBtn.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
			{
				// Reset methodSelected Buttons.
				deselectRobotSelectedButtons();

                iRobotCreateBtn.Background = new SolidColorBrush(columbiaBlue);
				robotSelected = "iRobot";
			}
			else // Tile Unchecked
			{
                iRobotCreateBtn.Background = new SolidColorBrush(royalBlue);
				robotSelected = "";
			}
		}

		private void deselectRobotSelectedButtons()
		{
			R2D2Btn.Background = new SolidColorBrush(royalBlue);
			OWIArmBtn.Background = new SolidColorBrush(royalBlue);
            iRobotCreateBtn.Background = new SolidColorBrush(royalBlue);
		}

		#endregion 2nd Column Tiles

		#region Train User

		/// <summary>
		/// Train Selected User
		/// </summary>
		private void TrainUserBtn_Click(object sender, RoutedEventArgs e)
		{
			// Check if Emotiv is turned on!!!!
			if (MainEmotivToggleSwitch.IsChecked == true)
			{ 
				if (methodSelected == "EEG")
				{
					// Navigate to the EEG Training Page
					pageFadeOut(currentPage);
					Storyboard Step1Load = (Storyboard)FindResource("CognitivTrainingPageIn");
					Step1Load.Begin(this);
					currentPage = 5;
				}
				else
				{
					MessageBox.Show("Please make sure that the EEG method is selected for training");
				}
			}
			else
			{
				MessageBox.Show("Please make sure that the Emotiv device is turned on");
			}
		}

		#endregion Train User

        #region Start

        private void StartBtn_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Check if Emotiv is turned on!!!!
            if ((MainEmotivToggleSwitch.IsChecked == true) && (robotSelected != "") && (methodSelected != ""))
            {
                if (methodSelected == "SSVEP")
                {
                    // Navigate to the SSVEP Page
                    pageFadeOut(currentPage);
                    Storyboard Step1Load = (Storyboard)FindResource("SSVEPPageIn");
                    Step1Load.Begin(this);
                    currentPage = 6;
                }
                else if (methodSelected == "EEG")
                {
                    // Navigate to the EEG  Page
                    pageFadeOut(currentPage);
                    Storyboard Step1Load = (Storyboard)FindResource("CognitivPageIn");
                    Step1Load.Begin(this);
                    currentPage = 7;
                }
                else // EMG Selected
                {
                    // Navigate to the EMG Page
                    pageFadeOut(currentPage);
                    Storyboard Step1Load = (Storyboard)FindResource("EMGPageIn");
                    Step1Load.Begin(this);
                    currentPage = 8;
                }
            }
            else
            {
                // Show check connectivity dialog
                CheckConnectivity dialog = new CheckConnectivity();
                dialog.ShowDialog();
            }
        }

        #endregion Start

		#endregion Main Menu

		#region Settings

		#region General Settings

		#region Emotiv Toggle Switch

		private void EmotivToggleSwitch_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// Turn Emotiv On
			if (EmotivToggleSwitch.IsChecked == true)
			{
				EmotivToggleSwitch.IsChecked = MainEmotivToggleSwitch.IsChecked = true;
				ConnectEmotiv();
			}
			else // Turn Emotiv Off
			{
				EmotivToggleSwitch.IsChecked = MainEmotivToggleSwitch.IsChecked = false;
				DisconnectEmotiv();
			}
		}

		private void MainEmotivToggleSwitch_Click(object sender, RoutedEventArgs e)
		{
			// Turn Emotiv On
			if (MainEmotivToggleSwitch.IsChecked == true)
			{
				EmotivToggleSwitch.IsChecked = MainEmotivToggleSwitch.IsChecked = true;
				ConnectEmotiv();
			}
			else // Turn Emotiv Off
			{
				EmotivToggleSwitch.IsChecked = MainEmotivToggleSwitch.IsChecked = false;
				DisconnectEmotiv();
			}
		}

		private void ConnectEmotiv()
		{
			// Try to connect the Emotiv device.

			// Connect Emotiv
			engine.EmoStateUpdated += new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
			engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
			engine.Connect();
			emotivDataCollectionTimer.Start();

			EmotivStatusLbl.Content = "Connected";          // Set to Connected
			emotivIsConnected = true;                       // Set emotivIsConnected to true
            System.Threading.Thread.Sleep(100);             // Use a system Thread Sleep to delay loading the user profile and result into not connected error.
            
			LoadUserProfile(UsersComboBox.SelectedIndex);   // Load User Profile
            UpdateCognitivSkillRatings();
			UpdateBatteryCapacityIcon(BatteryLevel);        // Get Battery Level
			UpdateSignalStrengthIcon(SignalStatus);         // Get Wireless Signal Strength

			x = y = 0;                                      // Reset Gyros
		}

		private void DisconnectEmotiv()
		{
			// Disconnect Emotiv
			engine.EmoStateUpdated -= new EmoEngine.EmoStateUpdatedEventHandler(engine_EmoStateUpdated);
			engine.UserAdded -= new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
			engine.Disconnect();
			emotivDataCollectionTimer.Stop();

			EmotivStatusLbl.Content = "Not Connected";      // Set to Disconnected
			emotivIsConnected = false;                      // Set emotivIsConnected to false
			UpdateBatteryCapacityIcon(0);                   // Set Battery Level
			UpdateSignalStrengthIcon("NO_SIGNAL");          // Set Wireless Signal Strength
			GyroStatus.Content = "Off";                     // Set Gyro Status to Off

			SamplingRate.Content = BufferSize.Content = "";

			ClearContacts();

			#region Clear Expressiv Data

			Smile.Content = Clench.Content = Eyebrow.Content = "";
			Smile1.Content = Clench1.Content = Eyebrow1.Content = "";
			ClearExpressivUpperFaceCircles();
			ClearExpressivLowerFaceCircles();

			#endregion Clear Expressiv Data
		}

		/// <summary>
		/// Turn off the color of the contacts to Black.
		/// </summary>
		private void ClearContacts()
		{
			AF3Contact.Fill = AF3ContactMain.Fill = Brushes.Black;
			AF4Contact.Fill = AF4ContactMain.Fill = Brushes.Black;
			F7Contact.Fill = F7ContactMain.Fill = Brushes.Black;
			F3Contact.Fill = F3ContactMain.Fill = Brushes.Black;
			F4Contact.Fill = F4ContactMain.Fill = Brushes.Black;
			F8Contact.Fill = F8ContactMain.Fill = Brushes.Black;
			FC5Contact.Fill = FC5ContactMain.Fill = Brushes.Black;
			FC6Contact.Fill = FC6ContactMain.Fill = Brushes.Black;
			T7Contact.Fill = T7ContactMain.Fill = Brushes.Black;
			T8Contact.Fill = T8ContactMain.Fill = Brushes.Black;
			CMSContact.Fill = CMSContactMain.Fill = Brushes.Black;
			DRLContact.Fill = DRLContactMain.Fill = Brushes.Black;
			P7Contact.Fill = P7ContactMain.Fill = Brushes.Black;
			P8Contact.Fill = P8ContactMain.Fill = Brushes.Black;
			O1Contact.Fill = O1ContactMain.Fill = Brushes.Black;
			O2Contact.Fill = O2ContactMain.Fill = Brushes.Black;
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

            for (int j = 0; j < 4; j++)
            {
				// Butterworth Filtered Data for every filter
                butAF3[j] = ButAF3[j].getFilteredValue(AF3, coefficients[j], gain[j]);
                butAF4[j] = ButAF4[j].getFilteredValue(AF4, coefficients[j], gain[j]);
                butF3[j] = ButF3[j].getFilteredValue(F3, coefficients[j], gain[j]);
                butF4[j] = ButF4[j].getFilteredValue(F4, coefficients[j], gain[j]);
                butF7[j] = ButF7[j].getFilteredValue(F7, coefficients[j], gain[j]);
                butF8[j] = ButF8[j].getFilteredValue(F8, coefficients[j], gain[j]);
                butFC5[j] = ButFC5[j].getFilteredValue(FC5, coefficients[j], gain[j]);
                butFC6[j] = ButFC6[j].getFilteredValue(FC6, coefficients[j], gain[j]);
                butO1[j] = ButO1[j].getFilteredValue(O1, coefficients[j], gain[j]);
                butO2[j] = ButO2[j].getFilteredValue(O2, coefficients[j], gain[j]);
                butP7[j] = ButP7[j].getFilteredValue(P7, coefficients[j], gain[j]);
                butP8[j] = ButP8[j].getFilteredValue(P8, coefficients[j], gain[j]);
                butT7[j] = ButT7[j].getFilteredValue(T7, coefficients[j], gain[j]);
                butT8[j] = ButT8[j].getFilteredValue(T8, coefficients[j], gain[j]);
            }

			#endregion Butterworth Filtered Data

            //#region Update EEG Values

            //switch (ElectrodeComboBox.Text)
            //{
            //    case "AF3":
            //        EEGval.Content = ((Int32)AF3).ToString();
            //        EEGbutval.Content = ((Int32)butAF3[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "F7":
            //        EEGval.Content = ((Int32)F7).ToString();
            //        EEGbutval.Content = ((Int32)butF7[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "F3":
            //        EEGval.Content = ((Int32)F3).ToString();
            //        EEGbutval.Content = ((Int32)butF3[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "FC5":
            //        EEGval.Content = ((Int32)FC5).ToString();
            //        EEGbutval.Content = ((Int32)butFC5[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "T7":
            //        EEGval.Content = ((Int32)T7).ToString();
            //        EEGbutval.Content = ((Int32)butT7[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "P7":
            //        EEGval.Content = ((Int32)P7).ToString();
            //        EEGbutval.Content = ((Int32)butP7[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "O1":
            //        EEGval.Content = ((Int32)O1).ToString();
            //        EEGbutval.Content = ((Int32)butO1[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "O2":
            //        EEGval.Content = ((Int32)O2).ToString();
            //        EEGbutval.Content = ((Int32)butO2[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "P8":
            //        EEGval.Content = ((Int32)P8).ToString();
            //        EEGbutval.Content = ((Int32)butP8[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "T8":
            //        EEGval.Content = ((Int32)T8).ToString();
            //        EEGbutval.Content = ((Int32)butT8[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "FC6":
            //        EEGval.Content = ((Int32)FC6).ToString();
            //        EEGbutval.Content = ((Int32)butFC6[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "F4":
            //        EEGval.Content = ((Int32)F4).ToString();
            //        EEGbutval.Content = ((Int32)butF4[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "F8":
            //        EEGval.Content = ((Int32)F8).ToString();
            //        EEGbutval.Content = ((Int32)butF8[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    case "AF4":
            //        EEGval.Content = ((Int32)AF4).ToString();
            //        EEGbutval.Content = ((Int32)butAF4[FilterComboBox.SelectedIndex]).ToString();
            //        break;
            //    default:
            //        break;
            //}
            
            
            //#endregion Update EEG Values


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
			FUNC_VALUE = data[EdkDll.EE_DataChannel_t.FUNC_VALUE][i];
			INTERPOLATED = data[EdkDll.EE_DataChannel_t.INTERPOLATED][i];
			MARKER = data[EdkDll.EE_DataChannel_t.MARKER][i];
			RAW_CQ = data[EdkDll.EE_DataChannel_t.RAW_CQ][i];
			SYNC_SIGNAL = data[EdkDll.EE_DataChannel_t.SYNC_SIGNAL][i];
			TIMESTAMP = data[EdkDll.EE_DataChannel_t.TIMESTAMP][i];

			#endregion Other Data

		}

		#endregion Update Emotiv Sensor Data

		#region Add Users

		/// <summary>
		/// Add User
		/// </summary>
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
				string newUserNamePath = emotivFilePath + newUserName + ".emu";

				// User gave a name
				if (newUserName != "")
				{
					// Name doesn't exist already
					if (!emuFilePaths.Contains(newUserNamePath))
					{
						// Save new user profile

						Profile P = EmoEngine.Instance.GetUserProfile(0);
						byte[] bytes = P.GetBytes();
						File.WriteAllBytes(newUserNamePath, bytes);

						// Refresh Comboboxes
						LoadUsers();

						// Load profile
						UsersComboBox.SelectedIndex = UsersComboBox.Items.IndexOf(newUserName);
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

		#endregion Add Users

		#region Remove Users

		/// <summary>
		/// Remove User
		/// </summary>
		private void RemoveUserBtn_Click(object sender, RoutedEventArgs e)
		{
			RemoveUser dialog = new RemoveUser();
			dialog.ShowDialog();

			if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
			{
				// Remove user
				File.Delete(emuFilePaths[UsersComboBox.SelectedIndex]);
				File.Delete(emuFilePaths[MainUsersComboBox.SelectedIndex]);
			}
			LoadUsers();
		}

		#endregion Remove Users

		#region Save User

		/// <summary>
		/// Save User
		/// </summary>
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

		/// <summary>
		/// Save the Users Profile
		/// </summary>
		private void SaveUserProfile()
		{
			string c = emotivFilePath + System.IO.Path.GetFileName(emuFilePaths[UsersComboBox.SelectedIndex]);
			//engine.EE_SaveUserProfile(userID, emotivFilePath + System.IO.Path.GetFileName(emuFilePaths[UsersComboBox.SelectedIndex]));
			//engine.EE_SaveUserProfile(userID, emuFilePaths[UsersComboBox.SelectedIndex]);

			Profile P = EmoEngine.Instance.GetUserProfile(0);
			byte[] bytes = P.GetBytes();
			File.WriteAllBytes(emuFilePaths[UsersComboBox.SelectedIndex], bytes);
			
		}

		#endregion Save Users

		#region Load Users

		/// <summary>
		/// Load Users
		/// </summary>
		private void LoadUsers()
		{
			UsersComboBox.Items.Clear();
			MainUsersComboBox.Items.Clear();
			emuFilePaths = Directory.GetFiles(emotivFilePath);
			foreach (var user in emuFilePaths)
			{
				UsersComboBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(user));
				MainUsersComboBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(user));
			}

			UsersComboBox.SelectedIndex = 0;
			MainUsersComboBox.SelectedIndex = 0;
		}

		/// <summary>
		/// Load Users Profiles
		/// </summary>
		private void LoadUserProfile(int SelectedIndex)
		{
			try
			{
                System.Threading.Thread.Sleep(5); // Gives ample time for system to load users and prevents device is not connected Message.
				engine.LoadUserProfile(userID, emuFilePaths[SelectedIndex]);
                UpdateCognitivSkillRatings();
			}
			catch (Exception)
			{
				MessageBox.Show("Device is not connected!");
			}
		}

		/// <summary>
		/// Users Combobox Value Changed
		/// </summary>
		private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((emotivIsConnected) && (UsersComboBox.SelectedIndex!=-1))
			{
				LoadUserProfile(UsersComboBox.SelectedIndex);
				
			}
			 MainUsersComboBox.SelectedIndex = UsersComboBox.SelectedIndex;
		}

		/// <summary>
		/// Users Combobox Value Changed
		/// </summary>
		private void MainUsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if ((emotivIsConnected) && (MainUsersComboBox.SelectedIndex != -1))
			{
				LoadUserProfile(MainUsersComboBox.SelectedIndex);
				
			}
			UsersComboBox.SelectedIndex = MainUsersComboBox.SelectedIndex;
		}

		#endregion Load Users

		#region Train Users

		private void SettingsTrainBtn_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// Check if Emotiv is turned on!!!!
			if (MainEmotivToggleSwitch.IsChecked == true)
			{
				// Navigate to the main Training Page
				pageFadeOut(currentPage);
				Storyboard Step1Load = (Storyboard)FindResource("TrainingPageIn");
				Step1Load.Begin(this);
				currentPage = 3;
			}
			else
			{
				MessageBox.Show("Please make sure that the device is turned on");
			}
		}

		

		#endregion Train Users

		#region Emotiv Event Handlers

		void engine_EmoStateUpdated(object sender, EmoStateUpdatedEventArgs e)
		{
			es = e.emoState;
			elapsed = es.GetTimeFromStart();
			UpdateSensorContactQuality(es);

			SignalStatus = es.GetWirelessSignalStatus().ToString();
            MainUptime.Content = ConvertToTime(es.GetTimeFromStart());
			Uptime.Content = ConvertToTime(es.GetTimeFromStart());
			es.GetBatteryChargeLevel(out BatteryLevel, out MaxBatteryLevel);
			SamplingRate.Content = samplingRate.ToString();
			BufferSize.Content = bufferSize.ToString();
			EdkDll.EE_DataGetBufferSizeInSec(out bufferSize);
			EdkDll.EE_DataGetSamplingRate(userID, out samplingRate);

			EdkDll.EE_ExpressivAlgo_t lowerFaceAction, upperFaceAction;
			bool expressivIsBlink, expressivIsEyesOpen, expressivIsLeftWink, expressivIsLookingDown, expressivIsLookingLeft, expressivIsLookingRight, expressivIsLookingUp, expressivIsRightWink;
			float lowerFaceActionPower, upperFaceActionPower;
			float leftEye, rightEye;

			#region Expressiv

			#region Lower Face Action

			lowerFaceAction = es.ExpressivGetLowerFaceAction();
			lowerFaceActionPower = es.ExpressivGetLowerFaceActionPower();

			switch (lowerFaceAction)
			{
				case EdkDll.EE_ExpressivAlgo_t.EXP_CLENCH:
					ClearExpressivLowerFaceCircles();
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_LAUGH:
					ClearExpressivLowerFaceCircles();
					LaughRect.Fill = Brushes.Green;
					LaughRect1.Fill = Brushes.Green;
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_NEUTRAL:
					ClearExpressivLowerFaceCircles();
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_SMILE:
					ClearExpressivLowerFaceCircles();
					SmileRect.Fill = Brushes.Green;
					SmileRect1.Fill = Brushes.Green;
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_LEFT:
					ClearExpressivLowerFaceCircles();
					SmirkLeftRect.Fill = Brushes.Green;
					SmirkLeftRect1.Fill = Brushes.Green;
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_RIGHT:
					ClearExpressivLowerFaceCircles();
					SmirkRightRect.Fill = Brushes.Green;
					SmirkRightRect1.Fill = Brushes.Green;
					break;
				default:
					break;
			}

			#region Clench Extent

			float clenchExtent = es.ExpressivGetClenchExtent();
			Clench.Content = toPercentString(clenchExtent);
			Clench1.Content = toPercentString(clenchExtent);

			if (clenchExtent < 0.20)
			{
				ClenchRect.Fill = Brushes.Black;
			}
			else if (clenchExtent < 0.40)
			{
				ClenchRect.Fill = Brushes.Red;
			}
			else if (clenchExtent < 0.60)
			{
				ClenchRect.Fill = Brushes.Orange;
			}
			else if (clenchExtent < 0.80)
			{
				ClenchRect.Fill = Brushes.Yellow;
			}
			else
			{
				ClenchRect.Fill = Brushes.Green;
			}

			#endregion Clench Extent

			#region Smile Extent

			float smileExtent = es.ExpressivGetSmileExtent();
			Smile.Content = toPercentString(smileExtent);
			Smile1.Content = toPercentString(smileExtent);

			if (smileExtent < 0.20)
			{
				SmileRect.Fill = Brushes.Black;
			}
			else if (smileExtent < 0.40)
			{
				SmileRect.Fill = Brushes.Red;
			}
			else if (smileExtent < 0.60)
			{
				SmileRect.Fill = Brushes.Orange;
			}
			else if (smileExtent < 0.80)
			{
				SmileRect.Fill = Brushes.Yellow;
			}
			else
			{
				SmileRect.Fill = Brushes.Green;
			}

			#endregion Smile Extent

			//LowerFaceAction.Content = lowerFaceAction.ToString();

			#endregion Lower Face Action

			#region Middle Face Action

			es.ExpressivGetEyelidState(out leftEye, out rightEye);
			expressivIsBlink = es.ExpressivIsBlink();
			if (expressivIsBlink)
			{
				ClearExpressivUpperFaceCircles();
				BlinkRect.Fill = Brushes.Green;
				BlinkRect1.Fill = Brushes.Green;
			}
			expressivIsEyesOpen = es.ExpressivIsEyesOpen();
			if (expressivIsEyesOpen)
			{
				//MiddleFaceAction.Content = "Eyes Open";
				BlinkRect.Fill = Brushes.Black;
				BlinkRect1.Fill = Brushes.Black;
			}
			else
			{
				BlinkRect.Fill = Brushes.Green;
				BlinkRect1.Fill = Brushes.Green;
				//MiddleFaceAction.Content = "Eyes Closed";
			}
			expressivIsLeftWink = es.ExpressivIsLeftWink();
			if (expressivIsLeftWink)
			{
				ClearExpressivUpperFaceCircles();
				WinkLeftRect.Fill = Brushes.Green;
				WinkLeftRect1.Fill = Brushes.Green;
			}
			expressivIsRightWink = es.ExpressivIsRightWink();
			if (expressivIsRightWink)
			{
				ClearExpressivUpperFaceCircles();
				WinkRightRect.Fill = Brushes.Green;
				WinkRightRect1.Fill = Brushes.Green;
			}
			expressivIsLookingDown = es.ExpressivIsLookingDown();
			if (expressivIsLookingDown)
			{
				//MessageBox.Show("I am looking down");
			}
			expressivIsLookingLeft = es.ExpressivIsLookingLeft();
			if (expressivIsLookingLeft)
			{
				//MessageBox.Show("I am looking left"); // Emotiv SDK is wrong
			}
			expressivIsLookingRight = es.ExpressivIsLookingRight();
			if (expressivIsLookingRight)
			{
				//MessageBox.Show("I am looking right"); // Emotiv SDK is wrong
			}
			expressivIsLookingUp = es.ExpressivIsLookingUp();
			if (expressivIsLookingUp)
			{
				//MessageBox.Show("I am looking up");
			}
			float eyeXCoordinate, eyeYCoordinate;
			es.ExpressivGetEyeLocation(out eyeXCoordinate, out eyeYCoordinate);
			if (eyeXCoordinate > 0)
			{
				//MessageBox.Show("Looking Right");
			}
			else if (eyeXCoordinate<0)
			{
				//MessageBox.Show("Looking Left"); // sometimes it works
			}
			if (eyeYCoordinate > 0)
			{
				MessageBox.Show("Looking Up"); // not working
			}
			else if (eyeYCoordinate < 0)
			{
				MessageBox.Show("Looking Down"); // not working
			}

			#endregion Middle Face Action

			#region Upper Face Action

			upperFaceAction = es.ExpressivGetUpperFaceAction();
			upperFaceActionPower = es.ExpressivGetUpperFaceActionPower();

			switch (upperFaceAction)
			{
				case EdkDll.EE_ExpressivAlgo_t.EXP_EYEBROW:
					ClearExpressivUpperFaceCircles();
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_FURROW:
					ClearExpressivUpperFaceCircles();
					FurrowRect.Fill = Brushes.Green;
					FurrowRect1.Fill = Brushes.Green;
					break;
				case EdkDll.EE_ExpressivAlgo_t.EXP_NEUTRAL:
					ClearExpressivUpperFaceCircles();
					break;
				default:
					break;
			}

			#region Eyebrow Extent

			float eyebrowExtent = es.ExpressivGetEyebrowExtent();
			Eyebrow.Content = toPercentString(eyebrowExtent);
			Eyebrow1.Content = toPercentString(eyebrowExtent);

			if (eyebrowExtent < 0.20)
			{
				EyebrowRect.Fill = Brushes.Black;
				EyebrowRect1.Fill = Brushes.Black;
			}
			else if (eyebrowExtent < 0.40)
			{
				EyebrowRect.Fill = Brushes.Red;
				EyebrowRect1.Fill = Brushes.Red;
			}
			else if (eyebrowExtent < 0.60)
			{
				EyebrowRect.Fill = Brushes.Orange;
				EyebrowRect1.Fill = Brushes.Orange;
			}
			else if (eyebrowExtent < 0.80)
			{
				EyebrowRect.Fill = Brushes.Yellow;
				EyebrowRect1.Fill = Brushes.Yellow;
			}
			else
			{
				EyebrowRect.Fill = Brushes.Green;
				EyebrowRect1.Fill = Brushes.Green;
			}

			#endregion Eyebrow Extent
						
			//UpperFaceAction.Content = upperFaceAction.ToString();

			#endregion Upper Face Action

			// Must be removed
			#region Robots

			#region OWI535 Robotic Arm

			//if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_CLENCH)
			//{
			//    OWI535RoboticArm.GrippersClose(1000);
			//}
			//else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_LEFT)
			//{
			//    OWI535RoboticArm.ArmRotateLeft(1000);
			//}
			//else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_RIGHT)
			//{
			//    OWI535RoboticArm.ArmRotateRight(1000);
			//}

			//else if (es.ExpressivGetEyebrowExtent() > 0.10)
			//{
			//    OWI535RoboticArm.ArmStop();
			//}
			//else if (es.ExpressivIsRightWink())
			//{
			//    OWI535RoboticArm.GrippersOpen(1000);
			//}

			#endregion OWI535 Robotic Arm

			#region R2D2

			if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_CLENCH)
			{
				//R2D2.MoveForward();
                int iRobotVelocity = getiRobotVelocity();
                iRobotMoveForward(iRobotVelocity);
                
			}
			else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_LEFT)
			{
				//R2D2.MoveLeft();
                int iRobotVelocity = getiRobotVelocity();
                iRobotTurnLeft(iRobotVelocity);
			}
			else if (lowerFaceAction == EdkDll.EE_ExpressivAlgo_t.EXP_SMIRK_RIGHT)
			{
				//R2D2.MoveRight();
                int iRobotVelocity = getiRobotVelocity();
                iRobotTurnRight(iRobotVelocity);
			}
			else if (es.ExpressivGetEyebrowExtent() > 0.10)
			{
				//R2D2.Stop();
                iRobotStop();
			}
			
			#endregion R2D2

			#endregion Robots

			#endregion Expressiv

			#region Affectiv
			
			AffectiveEngagementBoredom = es.AffectivGetEngagementBoredomScore();    // Get Engagement/Boredom Score.
			AffectivExcitementLong = es.AffectivGetExcitementLongTermScore();       // Get Excitement Long Term Score.
			AffectivExcitementShort = es.AffectivGetExcitementShortTermScore();     // Get Excitement Short Term Score.
			AffectivFrustration = es.AffectivGetFrustrationScore();                 // Get Frustration Score.
			AffectivMeditation = es.AffectivGetMeditationScore();                   // Get Meditation Score.

			#endregion Affectiv

			#region Cognitiv

			EdkDll.EE_CognitivAction_t EEGAction;
			EEGAction = es.CognitivGetCurrentAction();
			double cognitivpower = es.CognitivGetCurrentActionPower();
			double cognitivpowerPercent = cognitivpower * 100;
			UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(CognitivPowerProgressBar.SetValue);
			Dispatcher.Invoke(updatePbDelegate, System.Windows.Threading.DispatcherPriority.Background, new object[] { ProgressBar.ValueProperty, cognitivpowerPercent });

			cognitivPower.Content = (Convert.ToInt32(cognitivpowerPercent)).ToString() + " %";
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
						//R2D2.MoveBack();
                        int iRobotVelocity = getiRobotVelocity();
                        iRobotMoveForward(iRobotVelocity);
						//OWI535RoboticArm.Handshake();
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
			AF3Contact.Fill = AF3ContactMain.Fill = getContactQualityColor(contactQualityArray[3].ToString());
			AF4Contact.Fill = AF4ContactMain.Fill = getContactQualityColor(contactQualityArray[16].ToString());
			F7Contact.Fill = F7ContactMain.Fill = getContactQualityColor(contactQualityArray[4].ToString());
			F3Contact.Fill = F3ContactMain.Fill = getContactQualityColor(contactQualityArray[5].ToString());
			F4Contact.Fill = F4ContactMain.Fill = getContactQualityColor(contactQualityArray[14].ToString());
			F8Contact.Fill = F8ContactMain.Fill = getContactQualityColor(contactQualityArray[15].ToString());
			FC5Contact.Fill = FC5ContactMain.Fill = getContactQualityColor(contactQualityArray[6].ToString());
			FC6Contact.Fill = FC6ContactMain.Fill = getContactQualityColor(contactQualityArray[13].ToString());
			T7Contact.Fill = T7ContactMain.Fill = getContactQualityColor(contactQualityArray[7].ToString());
			T8Contact.Fill = T8ContactMain.Fill = getContactQualityColor(contactQualityArray[12].ToString());
			CMSContact.Fill = CMSContactMain.Fill = getContactQualityColor(contactQualityArray[0].ToString());
			DRLContact.Fill = DRLContactMain.Fill = getContactQualityColor(contactQualityArray[1].ToString());
			P7Contact.Fill = P7ContactMain.Fill = getContactQualityColor(contactQualityArray[8].ToString());
			P8Contact.Fill = P8ContactMain.Fill = getContactQualityColor(contactQualityArray[11].ToString());
			O1Contact.Fill = O1ContactMain.Fill = getContactQualityColor(contactQualityArray[9].ToString());
			O2Contact.Fill = O2ContactMain.Fill = getContactQualityColor(contactQualityArray[10].ToString());
		}

		// Clear all the Expressiv Checkboxes
		private void ClearExpressivUpperFaceCircles()
		{
			EyebrowRect.Fill = Brushes.Black;
			FurrowRect.Fill = Brushes.Black;
			WinkLeftRect.Fill = Brushes.Black;
			WinkRightRect.Fill = Brushes.Black;

			// EMG Ones
			EyebrowRect1.Fill = Brushes.Black;
			FurrowRect1.Fill = Brushes.Black;
			WinkLeftRect1.Fill = Brushes.Black;
			WinkRightRect1.Fill = Brushes.Black;

		}

		// Clear all the Expressiv Checkboxes
		private void ClearExpressivLowerFaceCircles()
		{
			ClenchRect.Fill = Brushes.Black;
			SmileRect.Fill = Brushes.Black;
			LaughRect.Fill = Brushes.Black;
			SmirkLeftRect.Fill = Brushes.Black;
			SmirkRightRect.Fill = Brushes.Black;

			ClenchRect1.Fill = Brushes.Black;
			SmileRect1.Fill = Brushes.Black;
			LaughRect1.Fill = Brushes.Black;
			SmirkLeftRect1.Fill = Brushes.Black;
			SmirkRightRect1.Fill = Brushes.Black;
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
				// Gyro Delta
				xValue.Content = x.ToString();
				yValue.Content = y.ToString();

				// Gyro Values
				xmax += x;
				ymax += y;
				xValueMax.Content = xmax.ToString();
				yValueMax.Content = ymax.ToString();

				GyroStatus.Content = "On";
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

		private void recalibrateGyroBtn_Click(object sender, RoutedEventArgs e)
		{
			x = y = 0;
			// Reset Gyro Delta
			xValue.Content = x.ToString();
			yValue.Content = y.ToString();

			// Reset Gyro Values
			xmax = x;
			ymax = 0 ;
			xValueMax.Content = xmax.ToString();
			yValueMax.Content = ymax.ToString();
		}

		#region Update Graphs

		/// <summary>
		/// Update Graphs
		/// </summary>
		private void UpdateGraphs()
		{

			#region EEG Graphs

            #region Raw EEG Graphs

            #region AF3Graph

            EEGChartDataObject AF3Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = AF3
            };
            AF3List.Add(AF3Obj);

            #endregion AF3Graph

            #region F7Graph

            EEGChartDataObject F7Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = F7
            };
            F7List.Add(F7Obj);

            #endregion F7Graph

            #region F3Graph

            EEGChartDataObject F3Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = F3
            };
            F3List.Add(F3Obj);

            #endregion F3Graph

            #region FC5Graph

            EEGChartDataObject FC5Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = FC5
            };
            FC5List.Add(FC5Obj);

            #endregion FC5Graph

            #region T7Graph

            EEGChartDataObject T7Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = T7
            };
            T7List.Add(T7Obj);

            #endregion T7Graph

            #region P7Graph

            EEGChartDataObject P7Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = P7
            };
            P7List.Add(P7Obj);

            #endregion T7Graph

            #region O1Graph

            EEGChartDataObject O1Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = O1
            };
            O1List.Add(O1Obj);

            #endregion O1Graph

            #region O2Graph

            EEGChartDataObject O2Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = O2
            };

            O2List.Add(O2Obj);

            #endregion O2Graph

            #region P8Graph

            EEGChartDataObject P8Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = P8
            };

            P8List.Add(P8Obj);

            #endregion P8Graph

            #region T8Graph

            EEGChartDataObject T8Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = T8
            };

            T8List.Add(T8Obj);

            #endregion T8Graph

            #region FC6Graph

            EEGChartDataObject FC6Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = FC6
            };

            FC6List.Add(FC6Obj);

            #endregion P8Graph

            #region F4Graph

            EEGChartDataObject F4Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = F4
            };

            F4List.Add(F4Obj);

            #endregion P8Graph

            #region F8Graph

            EEGChartDataObject F8Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = F8
            };

            F8List.Add(F8Obj);

            #endregion F8Graph

            #region AF4Graph

            EEGChartDataObject AF4Obj = new EEGChartDataObject
            {
                Time = DateTime.Now,
                Value = AF4
            };

            AF4List.Add(AF4Obj);

            #endregion AF4Graph

            #endregion Raw EEG Graphs

            #region Butterworth Graphs
            
            #endregion Butterworth Graphs

            LineSeries EEGSeries = (LineSeries)this.EEGChart.Series[0];
            EEGSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            EEGSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };

            LineSeries ButterworthSeries = (LineSeries)this.ButterworthChart.Series[0];
            ButterworthSeries.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            ButterworthSeries.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };

            switch (ElectrodeComboBox.Text)
            {
                case "AF3":
                    EEGval.Content = ((Int32)AF3).ToString();
                    EEGbutval.Content = ((Int32)butAF3[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = AF3List;
                    break;
                case "F7":
                    EEGval.Content = ((Int32)F7).ToString();
                    EEGbutval.Content = ((Int32)butF7[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = F7List;
                    break;
                case "F3":
                    EEGval.Content = ((Int32)F3).ToString();
                    EEGbutval.Content = ((Int32)butF3[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = AF3List;
                    break;
                case "FC5":
                    EEGval.Content = ((Int32)FC5).ToString();
                    EEGbutval.Content = ((Int32)butFC5[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = FC5List;
                    break;
                case "T7":
                    EEGval.Content = ((Int32)T7).ToString();
                    EEGbutval.Content = ((Int32)butT7[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = T7List;
                    break;
                case "P7":
                    EEGval.Content = ((Int32)P7).ToString();
                    EEGbutval.Content = ((Int32)butP7[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = P7List;
                    break;
                case "O1":
                    EEGval.Content = ((Int32)O1).ToString();
                    EEGbutval.Content = ((Int32)butO1[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = O1List;
                    break;
                case "O2":
                    EEGval.Content = ((Int32)O2).ToString();
                    EEGbutval.Content = ((Int32)butO2[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = O2List;
                    break;
                case "P8":
                    EEGval.Content = ((Int32)P8).ToString();
                    EEGbutval.Content = ((Int32)butP8[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = P8List;
                    break;
                case "T8":
                    EEGval.Content = ((Int32)T8).ToString();
                    EEGbutval.Content = ((Int32)butT8[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = T8List;
                    break;
                case "FC6":
                    EEGval.Content = ((Int32)FC6).ToString();
                    EEGbutval.Content = ((Int32)butFC6[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = FC6List;
                    break;
                case "F4":
                    EEGval.Content = ((Int32)F4).ToString();
                    EEGbutval.Content = ((Int32)butF4[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = F4List;
                    break;
                case "F8":
                    EEGval.Content = ((Int32)F8).ToString();
                    EEGbutval.Content = ((Int32)butF8[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = F8List;
                    break;
                case "AF4":
                    EEGval.Content = ((Int32)AF4).ToString();
                    EEGbutval.Content = ((Int32)butAF4[FilterComboBox.SelectedIndex]).ToString();
                    EEGSeries.ItemsSource = AF4List;
                    break;
                default:
                    break;
            }


			#region O2Graph

			EEGChartDataObject butAF4Obj = new EEGChartDataObject
			{
				Time = DateTime.Now,
				Value = butAF4[0]
			};

            butAF4List.Add(butAF4Obj);

			
            ButterworthSeries.ItemsSource = butAF4List;


            LineSeries AF3Series = (LineSeries)this.AF3Chart.Series[0];
            AF3Series.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            AF3Series.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            AF3Series.ItemsSource = AF3List;

            LineSeries F7Series = (LineSeries)this.F7Chart.Series[0];
            F7Series.CategoryBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Time"
            };
            F7Series.ValueBinding = new PropertyNameDataPointBinding()
            {
                PropertyName = "Value"
            };
            F7Series.ItemsSource = F7List;




			#endregion O2Graph

			#endregion EEG Graphs

			#region Affectiv Graphs

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

            #endregion Affectiv Graphs

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

			#region RawCQSignalAmplitudeGraph

			RawCQSignalAmplitudeChartDataObject RawCQSignalAmplitudeObj = new RawCQSignalAmplitudeChartDataObject
			{
				//Time = ConvertToTime(elapsed),
				Time = DateTime.Now,
				Value = RAW_CQ
			};
			RawCQSignalAmplitudeList.Add(RawCQSignalAmplitudeObj);

			LineSeries RawCQSignalAmplitudeSeries = (LineSeries)this.RawCQSignalAmplitudeChart.Series[0];
			RawCQSignalAmplitudeSeries.CategoryBinding = new PropertyNameDataPointBinding()
			{
				PropertyName = "Time"
			};
			RawCQSignalAmplitudeSeries.ValueBinding = new PropertyNameDataPointBinding()
			{
				PropertyName = "Value"
			};
			RawCQSignalAmplitudeSeries.ItemsSource = RawCQSignalAmplitudeList;

			#endregion RawCQSignalAmplitudeGraph

			#endregion Data Graphs
		}

		#endregion Update Graphs

		#endregion Specific Settings

		#endregion Settings

		#region Training

		#region Cognitiv Training

		/// <summary>
		/// Start Cognitiv Training Button
		/// </summary>
		private void StartCognitivTrainingBtn_Click(object sender, RoutedEventArgs e)
		{
            // Check if Emotiv is turned on!!!!
            if (MainEmotivToggleSwitch.IsChecked == true)
            {
                // Navigate to the EEG Training Page
                pageFadeOut(currentPage);
                Storyboard Step1Load = (Storyboard)FindResource("CognitivTrainingPageIn");
                Step1Load.Begin(this);
                currentPage = 5;
            }
            else
            {
                MessageBox.Show("Please make sure that the Emotiv device is turned on");
            }
		}

        void engine_CognitivTrainingCompleted(object sender, EmoEngineEventArgs e)
        {
            // Training Successfully completed
            SaveUserProfile();
            UpdateCognitivSkillRatings();
        }
         
        void engine_CognitivTrainingFailed(object sender, EmoEngineEventArgs e)
        {
            MessageBox.Show("Training Failed! Please try again");
        }

        void engine_CognitivTrainingRejected(object sender, EmoEngineEventArgs e)
        {
            // Nothing is saved.
        }

        void engine_CognitivTrainingSucceeded(object sender, EmoEngineEventArgs e)
        {
            TrainingCompleted dialog = new TrainingCompleted();
            dialog.ShowDialog();

            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                // Accept Training.
                engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_ACCEPT);
            }
            else
            {
                // Reject Training.
                engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_REJECT);
            }

        }

        void engine_CognitivTrainingStarted(object sender, EmoEngineEventArgs e)
        {

        }

        private void UpdateCognitivSkillRatings()
        {
            // Update Overall Skill Rating
            //MessageBox.Show(Convert.ToInt32(engine.CognitivGetOverallSkillRating(userID) * 100).ToString());

            CognitivPushTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_PUSH), 2) * 100).ToString() + "%";
            CognitivPullTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_PULL), 2) * 100).ToString() + "%";
            CognitivLiftTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_LIFT), 2) * 100).ToString() + "%";
            CognitivDropTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_DROP), 2) * 100).ToString() + "%";
            CognitivLeftTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_LEFT), 2) * 100).ToString() + "%";
            CognitivRightTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_RIGHT), 2) * 100).ToString() + "%";
            CognitivRotateLeftTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_LEFT), 2) * 100).ToString() + "%";
            CognitivRotateRightTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_RIGHT), 2) * 100).ToString() + "%";
            CognitivRotateClockwiseTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_CLOCKWISE), 2) * 100).ToString() + "%";
            CognitivRotateAntiClockwiseTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_COUNTER_CLOCKWISE), 2) * 100).ToString() + "%";
            CognitivRotateForwardsTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_FORWARDS), 2) * 100).ToString() + "%";
            CognitivRotateBackwardsTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_REVERSE), 2) * 100).ToString() + "%";
            CognitivDisappearTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_DISAPPEAR), 2) * 100).ToString() + "%";
            CognitivNeutralTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL), 2) * 100).ToString() + "%";
            // Not Sure
            CognitivLongNeutralTrainingTile.Count = (Math.Round(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL), 2) * 100).ToString() + "%";
        }

        #region Cognitiv Training Tiles

        private void deselectCognitivTileSelectedButtons()
        {
            CognitivPushTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivPullTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivLiftTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivDropTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivLeftTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRightTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateLeftTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateRightTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateClockwiseTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateAntiClockwiseTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateForwardsTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivRotateBackwardsTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivDisappearTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivNeutralTrainingTile.Background = new SolidColorBrush(royalBlue);
            CognitivLongNeutralTrainingTile.Background = new SolidColorBrush(royalBlue);
        }

        #region Push

        private void CognitivPushTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivPushTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivPushTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Push";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_PUSH);
            }
            else // Tile Unchecked
            {
                CognitivPushTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }

            
            //// Ask to Train Cognitiv
            
            //engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_START);

            //engine.CognitivTrainingStarted += engine_CognitivTrainingStarted;
            //engine.CognitivTrainingSucceeded += engine_CognitivTrainingSucceeded;
            //engine.CognitivTrainingRejected += engine_CognitivTrainingRejected;
            //engine.CognitivTrainingFailed += engine_CognitivTrainingFailed;
            //engine.CognitivTrainingCompleted += engine_CognitivTrainingCompleted;
        }

        #endregion Push

        #region Pull

        private void CognitivPullTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivPullTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivPullTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Pull";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_PULL);
            }
            else // Tile Unchecked
            {
                CognitivPullTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Pull

        #region Lift

        private void CognitivLiftTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivLiftTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivLiftTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Lift";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_LIFT);
            }
            else // Tile Unchecked
            {
                CognitivLiftTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Lift

        #region Drop

        private void CognitivDropTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivDropTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivDropTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Drop";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_DROP);
            }
            else // Tile Unchecked
            {
                CognitivDropTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Drop

        #region Left

        private void CognitivLeftTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivLeftTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivLeftTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Left";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_LEFT);
            }
            else // Tile Unchecked
            {
                CognitivLeftTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Left

        #region Right

        private void CognitivRightTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRightTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRightTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Right";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_RIGHT);
            }
            else // Tile Unchecked
            {
                CognitivRightTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion  Right

        #region Rotate Left

        private void CognitivRotateLeftTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateLeftTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateLeftTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate Left";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_LEFT);
            }
            else // Tile Unchecked
            {
                CognitivRotateLeftTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate Left

        #region Rotate Right

        private void CognitivRotateRightTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateRightTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateRightTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate Right";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_RIGHT);
            }
            else // Tile Unchecked
            {
                CognitivRotateRightTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate Right

        #region Rotate Clockwise

        private void CognitivRotateClockwiseTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateClockwiseTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateClockwiseTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate Clockwise";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_CLOCKWISE);
            }
            else // Tile Unchecked
            {
                CognitivRotateClockwiseTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate Clockwise

        #region Rotate AntiClockwise

        private void CognitivRotateAntiClockwiseTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateAntiClockwiseTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateAntiClockwiseTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate AntiClockwise";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_COUNTER_CLOCKWISE);
            }
            else // Tile Unchecked
            {
                CognitivRotateAntiClockwiseTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate AntiClockwise

        #region Rotate Forwards

        private void CognitivRotateForwardsTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateForwardsTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateForwardsTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate Forwards";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_FORWARDS);
            }
            else // Tile Unchecked
            {
                CognitivRotateForwardsTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate Forwards

        #region Rotate Backwards

        private void CognitivRotateBackwardsTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivRotateBackwardsTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivRotateBackwardsTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Rotate Reverse";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_ROTATE_REVERSE);
            }
            else // Tile Unchecked
            {
                CognitivRotateBackwardsTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Rotate Backwards

        #region Disappear

        private void CognitivDisappearTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivDisappearTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivDisappearTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Dissapear";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_DISAPPEAR);
            }
            else // Tile Unchecked
            {
                CognitivDisappearTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Disappear

        #region Neutral

        private void CognitivNeutralTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivNeutralTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivNeutralTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Neutral";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
            else // Tile Unchecked
            {
                CognitivNeutralTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
        }

        #endregion Neutral

        // Not Sure
        #region Long Neutral

        private void CognitivLongNeutralTrainingTile_Click(object sender, RoutedEventArgs e)
        {
            if ((CognitivLongNeutralTrainingTile.Background as SolidColorBrush).Color != columbiaBlue) // Tile Checked
            {
                // Reset methodSelected Buttons.
                deselectCognitivTileSelectedButtons();

                CognitivLongNeutralTrainingTile.Background = new SolidColorBrush(columbiaBlue);
                trainingTileSelected = "Long Neutral";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
            else // Tile Unchecked
            {
                CognitivLongNeutralTrainingTile.Background = new SolidColorBrush(royalBlue);
                trainingTileSelected = "";
                engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_NEUTRAL);
            }
            ///
        }

        #endregion Long Neutral

        #region Train Tile

        private void CognitivTrainTile_Click(object sender, RoutedEventArgs e)
        {
            //engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_START);

            MessageBox.Show(Convert.ToInt32(engine.CognitivGetOverallSkillRating(userID) * 100).ToString());
            //MessageBox.Show(engine.CognitivGetActionSkillRating(userID, EdkDll.EE_CognitivAction_t.COG_PUSH).ToString());

            //engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_START);
            //MessageBox.Show(engine.CognitivGetActiveActions(userID).ToString());
            //engine.CognitivSetTrainingAction(userID, EdkDll.EE_CognitivAction_t.COG_PUSH);


            MessageBox.Show(engine.CognitivGetTrainingAction(userID).ToString());

            MessageBox.Show(engine.CognitivGetActiveActions(userID).ToString());

            engine.CognitivSetTrainingControl(userID, EdkDll.EE_CognitivTrainingControl_t.COG_START);

            //engine.CognitivSetActivationLevel(userID, 3);
            //EdkDll.EE_CognitivAction_t actions =
            //EdkDll.EE_CognitivAction_t.COG_LEFT
            //| EdkDll.EE_CognitivAction_t.COG_PULL
            //| 
            //EdkDll.EE_CognitivAction_t.COG_PUSH;
            //EmoEngine.Instance.CognitivSetActiveActions(0, (UInt32)actions); 

            
            engine.CognitivTrainingStarted += engine_CognitivTrainingStarted;
            engine.CognitivTrainingSucceeded += engine_CognitivTrainingSucceeded;
            engine.CognitivTrainingRejected += engine_CognitivTrainingRejected;
            engine.CognitivTrainingFailed += engine_CognitivTrainingFailed;
            engine.CognitivTrainingCompleted += engine_CognitivTrainingCompleted;
        }

        #endregion Train Tile

        #endregion Cognitiv Training Tiles

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Camera3DPerspectiveCamera.Transform.Transform()
            //Camera3DYaw.Angle += 10;
            Move3DCubeUp(5);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Rotate3DCubeClockwise(10);
        }

		#endregion Cognitiv Training

        #region Expressiv Training

        // To be filled.

        #endregion Expressiv Training

        #endregion Training

        #region Expressiv (EMG)
        #endregion Expressiv (EMG)

        #region Cognitiv (EEG)
        #endregion Cognitiv (EEG)

        #region SSVEP (EEG)
        #endregion SSVEP (EEG)

        #region Helper Functions

        #region Timers

        #region Regular Timers
        #endregion Regular Timers

        #region Training Timers

        #region Training Cognitiv Timer

        DispatcherTimer trainingCognitivTimer = new DispatcherTimer();

        /// <summary>
        /// Start flashing.
        /// </summary>
        /// <param name="Hz"></param>
        public void trainingCognitivTimerStart(int Hz)
        {
            trainingCognitivTimer.Tick += trainingCognitivTimer_Tick;
            int ms = convertHzToMs(Hz);
            trainingCognitivTimer.Interval = new TimeSpan(0, 0, 0, 0, ms / 2);
            trainingCognitivTimer.IsEnabled = true;
        }

        /// <summary>
        /// Stop flashing.
        /// </summary>
        public void trainingCognitivTimerStop()
        {
            trainingCognitivTimer.Tick -= trainingCognitivTimer_Tick;
            trainingCognitivTimer.IsEnabled = false;
            trainingCognitivTimer.Stop();
        }

        void trainingCognitivTimer_Tick(object sender, EventArgs e)
        {
            //CognitivTrainingRectangle.Fill = new SolidColorBrush(Flash((SolidColorBrush)SSVEPTrainingRectangle.Fill));
        }

        #endregion Training Cognitiv Timer

        #endregion Training Timers

        #endregion Timers

        /// <summary>
        /// Converts the Frequency to Time
        /// </summary>
        /// <param name="Hz">The Frequency in Hz</param>
        /// <returns>Returns the time in miliseconds</returns>
        public int convertHzToMs(int Hz)
        {
            return 1 / Hz * 1000;
        }
              

		private string toPercentString(float value)
		{
			return (Convert.ToInt32(value * 100)).ToString();
		}



		#region 3D Cube Transformations

		#region Pitch

		private void Rotate3DCubeForward(int value)
		{
			Camera3DPitch.Angle += value;
		}

		private void Rotate3DCubeBackward(int value)
		{
			Camera3DPitch.Angle -= value;
		}

		#endregion Pitch

		#region Yaw

		/// <summary>
		/// Rotate the 3D Cube Left
		/// </summary>
		/// <param name="value">value to rotate</param>
		private void Rotate3DCubeLeft(int value)
		{
			Camera3DYaw.Angle += value;
		}

		/// <summary>
		/// Rotate the 3D Cube Right
		/// </summary>
		/// <param name="value">value to rotate</param>
		private void Rotate3DCubeRight(int value)
		{
			Camera3DYaw.Angle -= value;
		}

		#endregion Yaw

		#region Roll

		/// <summary>
		/// Rotate the 3D Cube Clockwise
		/// </summary>
		/// <param name="value">value to rotate</param>
		private void Rotate3DCubeClockwise(int value)
		{
			Camera3DRoll.Angle += value;
		}

		/// <summary>
		/// Rotate the 3D Cube AntiClockwise
		/// </summary>
		/// <param name="value">value to rotate</param>
		private void Rotate3DCubeAntiClockwise(int value)
		{
			Camera3DRoll.Angle -= value;
		}

		#endregion Roll

		#region Move X

		/// <summary>
		/// Move 3D Cube Left
		/// </summary>
		/// <param name="x">x value</param>
		private void Move3DCubeLeft(double x)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X + x, Cube3DPerspectiveCamera.Position.Y, Cube3DPerspectiveCamera.Position.Z);
		}

		/// <summary>
		/// Move 3D Cube Right
		/// </summary>
		/// <param name="x">x value</param>
		private void Move3DCubeRight(double x)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X - x, Cube3DPerspectiveCamera.Position.Y, Cube3DPerspectiveCamera.Position.Z);
		}

		#endregion Move X

		#region Move Y

		/// <summary>
		/// Move 3D Cube Down
		/// </summary>
		/// <param name="y">y value</param>
		private void Move3DCubeDown(double y)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X, Cube3DPerspectiveCamera.Position.Y + y, Cube3DPerspectiveCamera.Position.Z);
		}

		/// <summary>
		/// Move 3D Cube Up
		/// </summary>
		/// <param name="y">y value</param>
		private void Move3DCubeUp(double y)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X, Cube3DPerspectiveCamera.Position.Y - y, Cube3DPerspectiveCamera.Position.Z);
		}

		#endregion Move Y

		#region Move Z

		/// <summary>
		/// Move 3D Cube In
		/// </summary>
		/// <param name="z">z value</param>
		private void Move3DCubeIn(double z)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X, Cube3DPerspectiveCamera.Position.Y, Cube3DPerspectiveCamera.Position.Z + z);
		}

		/// <summary>
		/// Move 3D Cube Out
		/// </summary>
		/// <param name="z">z value</param>
		private void Move3DCubeOut(double z)
		{
			Cube3DPerspectiveCamera.Position = new System.Windows.Media.Media3D.Point3D(Cube3DPerspectiveCamera.Position.X, Cube3DPerspectiveCamera.Position.Y, Cube3DPerspectiveCamera.Position.Z - z);
		}

		#endregion Move Z

		#region Transparency

		//Brush3DCube.Opacity = 50;
		#endregion Transparency

		#endregion 3D Cube Transformations

        #endregion Helper Functions

        #region Robots

        #region R2D2

        /// <summary>
        /// Connect to R2D2
        /// </summary>
        private void R2D2ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            R2D2.ConnectNXT(R2D2ComPort);
            R2D2.showCULogo();
        }

        /// <summary>
        /// Disconnect R2D2
        /// </summary>
        private void R2D2ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            R2D2.DisconnectNXT();
        }

        /// <summary>
        /// R2D2 Move Forward
        /// </summary>
        private void R2D2MoveForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.MoveForward();
        }

        /// <summary>
        /// R2D2 Turn Left
        /// </summary>
        private void R2D2TurnLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.MoveLeft();
        }

        /// <summary>
        /// R2D2 Turn Right
        /// </summary>
        private void R2D2TurnRightBtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.MoveRight();
        }

        /// <summary>
        /// R2D2 Move Back
        /// </summary>
        private void R2D2MoveBackBtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.MoveBack();
        }

        /// <summary>
        /// R2D2 Stop
        /// </summary>
        private void R2D2StopBtn_Click(object sender, RoutedEventArgs e)
        {
            R2D2.Stop();
        }

        #endregion R2D2

        #region OWI 535

        private void Light_Click(object sender, RoutedEventArgs e)
        {
            // Turn Lights On if OWI 535 Arm is connected & ToggleSwitch is toggled On.
            if (Light.IsChecked == true)
            {
                try
                {
                    OWI535RoboticArm.LightsOn();
                }
                catch (Exception)
                {
                    Light.IsChecked = false;
                    MessageBox.Show("OWI 535 Robotic Arm is not connected");
                }
            }
            else // Turn Lights Off if OWI 535 Arm is connected & ToggleSwitch is toggled Off.
            {
                try
                {
                    OWI535RoboticArm.LightsOff();
                }
                catch (Exception)
                {
                    MessageBox.Show("OWI 535 Robotic Arm is not connected");
                }
            }
        }

        private void Grippers_Click(object sender, RoutedEventArgs e)
        {
            // Open Grippers if OWI 535 Arm is connected & ToggleSwitch is toggled On.
            if (Grippers.IsChecked == true)
            {
                try
                {
                    OWI535RoboticArm.GrippersOpen(getArmSeconds(SecondsTxtBox.Text));
                }
                catch (Exception)
                {
                    Grippers.IsChecked = false;
                    MessageBox.Show("OWI 535 Robotic Arm is not connected");
                }
            }
            else // Close Grippers if OWI 535 Arm is connected & ToggleSwitch is toggled Off.
            {
                try
                {
                    OWI535RoboticArm.GrippersClose(getArmSeconds(SecondsTxtBox.Text));
                }
                catch (Exception)
                {
                    MessageBox.Show("OWI 535 Robotic Arm is not connected");
                }
            }
        }

        /// <summary>
        /// Moves the Grippers Up
        /// </summary>
        private void GrippersUpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.GrippersUp(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Moves the Grippers Down
        /// </summary>
        private void GrippersDownBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.GrippersDown(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Moves the Elbow Up
        /// </summary>
        private void ElbowUpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ElbowUp(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Moves the Elbow Down
        /// </summary>
        private void ElbowownBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ElbowDown(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Moves the Arm Up
        /// </summary>
        private void ArmUpBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ArmUp(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Moves the Arm Down
        /// </summary>
        private void ArmDownBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ArmDown(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Rotates the Arm Left
        /// </summary>
        private void ArmRotateLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ArmRotateLeft(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Rotates the Arm Right
        /// </summary>
        private void ArmRotateRightBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.ArmRotateRight(getArmSeconds(SecondsTxtBox.Text));
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        /// <summary>
        /// Returns the arm seconds
        /// </summary>
        /// <param name="ArmSecondsStringValue">The arm seconds string value.</param>
        /// <returns></returns>
        private int getArmSeconds(string armsSecondsStringValue)
        {
            int seconds;
            Int32.TryParse(armsSecondsStringValue, out seconds);
            return (seconds * 1000);
        }

        /// <summary>
        /// Performs Handshake gesture
        /// </summary>
        private void HandshakeBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OWI535RoboticArm.Handshake();
            }
            catch (Exception)
            {
                MessageBox.Show("OWI 535 Robotic Arm is not connected");
            }
        }

        #endregion OWI 535

        #region Talos

        private void TalosToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TalosMoveForwardBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TalosMoveBackBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TalosTurnLeftBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TalosTurnRightBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TalosStopBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion Talos

        #endregion Robots

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ButAF4[0].reset();
        }

        private void iRobotCreateToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(iRobotConnect(ComPortTxtBox.Text));
            MessageBox.Show(iRobotConnect("COM" + ComPortTxtBox.Text));
            _mjpeg.ParseStream(new Uri("http://192.168.11.6/videostream.cgi?user=admin&pwd=&resolution=32&rate=0"));
        }

        private void iRobotCreateToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            iRobotDisconnect();
        }

        private void iRobotCreateMoveForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            int iRobotVelocity = getiRobotVelocity();
            iRobotMoveForward(iRobotVelocity);
        }

        private void iRobotCreateMoveBackBtn_Click(object sender, RoutedEventArgs e)
        {
            int iRobotVelocity = getiRobotVelocity();
            iRobotMoveBackward(iRobotVelocity);
        }

        private void iRobotCreateTurnLeftBtn_Click(object sender, RoutedEventArgs e)
        {
            int iRobotVelocity = getiRobotVelocity();
            iRobotTurnLeft(iRobotVelocity);
        }

        private void iRobotCreateTurnRightBtn_Click(object sender, RoutedEventArgs e)
        {
            int iRobotVelocity = getiRobotVelocity();
            iRobotTurnRight(iRobotVelocity);
        }

        private void iRobotCreateStopBtn_Click(object sender, RoutedEventArgs e)
        {
            iRobotStop();
        }

        /// <summary>
        /// Get iRobot Create Velocity.
        /// </summary>
        /// <returns></returns>
        private int getiRobotVelocity()
        {
            int velocity;
            Int32.TryParse(VelocityTxtBox.Text, out velocity);
            return (velocity * 10);
        }

        #region iRobot create

        public string iRobotConnect(string COMPort)
        {
            if (!Functions.irobot.IsOpen())
            {
                return Functions.iRobotTryConnect(COMPort);
            }
            return "";
        }

        public string iRobotDisconnect()
        {
            if (Functions.irobot.IsOpen())
            {
                return Functions.iRobotTryDisconnect();
            }
            return "";
        }

        public void iRobotMoveForward(int iRobotVelocity)
        {
            Functions.iRobotMoveForward(iRobotVelocity);
        }

        public void iRobotMoveBackward(int iRobotVelocity)
        {
            Functions.iRobotMoveBackward(iRobotVelocity);
        }

        public void iRobotTurnLeft(int iRobotVelocity)
        {
            Functions.iRobotTurnLeft(iRobotVelocity);
        }

        public void iRobotTurnRight(int iRobotVelocity)
        {
            Functions.iRobotTurnRight(iRobotVelocity);
        }

        public void iRobotStop()
        {
            Functions.iRobotStop();
        }

        #endregion iRobot create

        
	}
}
