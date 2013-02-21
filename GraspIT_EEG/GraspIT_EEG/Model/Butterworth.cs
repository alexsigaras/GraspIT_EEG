using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GraspIT_EEG.Model
{
    /// <summary>
    /// This is a Butterworth Bandpass Filter class of order 3.
    /// nzeros and npoles are both 6 (2N)
    /// </summary>
    class Butterworth
    {
        
        public Butterworth()
        { }

        private double[] xv = new double[7];
        private double[] yv = new double[7];

        /// <summary>
        /// Butterworth Bandpass Filter of order 3.
        /// </summary>
        /// <param name="prefilteredValue">The raw EEG value prior to filtering.</param>
        /// <param name="coefficients">The coeficients array for the given bandpass frequencies.</param>
        /// <param name="gain">The gain for the given bandpass frequencies.</param>
        /// <returns>Returns the filtered value from a Butterworth Filter bandpass of order 3.</returns>
        public double getFilteredValue(double prefilteredValue, double[] coefficients, double gain)
        {
            xv[0] = xv[1]; xv[1] = xv[2]; xv[2] = xv[3]; xv[3] = xv[4]; xv[4] = xv[5]; xv[5] = xv[6];
            xv[6] = prefilteredValue / gain;

            yv[0] = yv[1]; yv[1] = yv[2]; yv[2] = yv[3]; yv[3] = yv[4]; yv[4] = yv[5]; yv[5] = yv[6];
            yv[6] = (xv[6] - xv[0])+ 3 * (xv[2] - xv[4])
                + (coefficients[0] * yv[0]) + (coefficients[1] * yv[1])
                + (coefficients[2] * yv[2]) + (coefficients[3] * yv[3])
                + (coefficients[4] * yv[4]) + (coefficients[5] * yv[5]);
            return yv[6];
        }

        /// <summary>
        /// Reset all the stored values for xv and yv.
        /// </summary>
        public void reset()
        {
            xv[0] = xv[1] = xv[2] = xv[3] = xv[4] = xv[5] = xv[6] = 0;
            yv[0] = yv[1] = yv[2] = yv[3] = yv[4] = yv[5] = yv[6] = 0;
        }

    }
}
