using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GraspIT_EEG.Model;
using GraspIT_EEG.Model.iRobot;

namespace GraspIT_EEG
{
    class Functions
    {

        #region Declarations

        public static int iRobotCurrentSpeed = 0;

        // Create Thread arrays for the robot.
        public static Thread[] iRobotThreadArray = new Thread[1];
       

        /// <summary>
        /// iRobot Create Constructor.
        /// </summary>
        public static iRobot irobot = new iRobot();

        #endregion Declarations

        #region iRobot Create

        #region iRobot Connections

        /// <summary>
        /// Try to connect to iRobot Create.
        /// </summary>
        public static string iRobotTryConnect(string COMPort)
        {

            string output = "";
            try
            {
                if (irobot.Connect(COMPort))
                {
                    output = "iRobot Connected...";
                }
            }
            catch
            {
                output = "No iRobot found on this port!\r\n";
            }

            return output;
        }

        /// <summary>
        /// Try to disconnect from iRobot Create.
        /// </summary>
        public static string iRobotTryDisconnect()
        {
            string output = "";
            if (!irobot.IsOpen())
                return output;
            try
            {
                irobot.Disconnect();
                output = "Disconnected!!\r\n";
            }
            catch
            {
                output = "Error disconnecting";
            }
            return output;
        }

        #endregion iRobot Connections

        #region iRobot Move Functions

        /// <summary>
        /// iRobot Create move forward at a given Velocity.
        /// </summary>
        public static void iRobotMoveForward(int velocity)
        {
            if (irobot.IsOpen())
            {
                iRobotThreadArray[0] = new Thread(() => irobot.move(velocity, 0));
                iRobotThreadArray[0].Start();
            }
        }

        /// <summary>
        /// iRobot Create move backward at a given Velocity.
        /// </summary>
        public static void iRobotMoveBackward(int velocity)
        {
            if (irobot.IsOpen())
            {
                iRobotThreadArray[0] = new Thread(() => irobot.move(-velocity, 0));
                iRobotThreadArray[0].Start();
            }
        }

        /// <summary>
        /// iRobot Create Turn Left.
        /// </summary>
        public static void iRobotTurnLeft(int velocity)
        {
            if (irobot.IsOpen())
            {
                iRobotThreadArray[0] = new Thread(() => irobot.moveLeft(velocity));
                iRobotThreadArray[0].Start();
            }
        }

        /// <summary>
        /// iRobot Create Turn Right.
        /// </summary>
        public static void iRobotTurnRight(int velocity)
        {
            if (irobot.IsOpen())
            {
                iRobotThreadArray[0] = new Thread(() => irobot.moveRight(velocity));
                iRobotThreadArray[0].Start();
            }
        }

        /// <summary>
        /// iRobot Create Stop.
        /// </summary>
        public static void iRobotStop()
        {
            if (irobot.IsOpen())
            {
                iRobotThreadArray[0] = new Thread(() => irobot.moveStop());
                iRobotThreadArray[0].Start();
            }
        }

        #endregion iRobot Move Functions

        #endregion iRobot Create
    }
}

