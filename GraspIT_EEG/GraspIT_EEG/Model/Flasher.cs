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
using System.Threading;
using System.Windows.Threading;

namespace GraspIT_EEG.Model
{
    class Flasher
    {
        DispatcherTimer timer = new DispatcherTimer();
        public SolidColorBrush bgColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        public SolidColorBrush flashColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public SolidColorBrush currentColor = new SolidColorBrush();
        public int Hz;

        public Flasher(int hz)
        {
            Hz = hz;
        }

        public Flasher(int hz, SolidColorBrush bgcolor, SolidColorBrush flashcolor) 
        {
            Hz = hz;
            bgColor = bgcolor;
            flashColor = flashcolor;
        }

        /// <summary>
        /// Start flashing.
        /// </summary>
        /// <param name="Hz"></param>
        public void start(int Hz)
        {
            timer.Tick += timer_Tick;
            int ms = convertHzToMs(Hz);
            timer.Interval = new TimeSpan(0, 0, 0, 0, ms/2);
            timer.IsEnabled = true;
        }

        /// <summary>
        /// Stop flashing.
        /// </summary>
        public void stop()
        {
            timer.Tick -= timer_Tick;
            timer.IsEnabled = false;
            timer.Stop();
        }

        /// <summary>
        /// Converts the Frequency to Time
        /// </summary>
        /// <param name="Hz">The Frequency in Hz</param>
        /// <returns>Returns the time in miliseconds</returns>
        public int convertHzToMs(int Hz)
        {
            return 1 / Hz * 1000;
        }

        /// <summary>
        /// Flash function
        /// </summary>
        /// <param name="currentColor">The current color of the object</param>
        /// <returns>The flashing color</returns>
        public SolidColorBrush Flash(SolidColorBrush currentColor)
        {
            if (currentColor == bgColor)
            {
                currentColor = flashColor;
            }
            else
            {
                currentColor = bgColor;
            }
            return currentColor;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Flash(currentColor);
        }
    }
}
