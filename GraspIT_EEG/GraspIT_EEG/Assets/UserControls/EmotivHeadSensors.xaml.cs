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

namespace EmotivBCIWPF.Assets.UserControls
{
    /// <summary>
    /// Interaction logic for EmotivHeadSensors.xaml
    /// </summary>
	
	[TemplateVisualState(Name = "Black", GroupName = "SensorStates")]
    [TemplateVisualState(Name = "Red", GroupName = "SensorStates")]
    [TemplateVisualState(Name = "Yellow", GroupName = "SensorStates")]
    [TemplateVisualState(Name = "Green", GroupName = "SensorStates")]
    public partial class EmotivHeadSensors : UserControl
    {
        public EmotivHeadSensors()
        {
            InitializeComponent();
        }
		
        //public enum SensorStates()
        //{
        //    Black = 0,
        //    Red = 1,
        //    Yellow = 2,
        //    Green = 3
        //}
		
        //public SensorStates State
        //{
        //    get { return (SensorStates)GetValue(StateProperty); }
        //    set { SetValue(StateProperty, value); }
        //}

        //public static readonly DependencyProperty StateProperty =
        //    DependencyProperty.Register(
        //        "State",
        //        typeof(SensorStates),
        //        typeof(Sensor),
        //        new PropertyMetadata(SensorsStates.Black,
        //        OnSensorStateChanged)
        //    );

        //    private static void OnSensorStateChanged(DependencyObject d,
        //                    DependencyPropertyChangedEventArgs e)
        //    {
        //        ((Sensor)d).SetState();
        //    }
			
        //    private void SetState(){}
    }
}
