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
 * - Lego Mindstorms NXT (R2D2) Library
  */

using System;
// Lego Mindstorms NXT Library
using NKH.MindSqualls;

namespace GraspIT_EEG.Model.Robots
{
    public static class R2D2
    {
        public static NxtBrick Brick;
        public static NxtMotorSync MotorPair;
        public static NxtBluetoothConnection conn;
        public static string ComPort;
        private static byte comPort;
        public static bool isConnected = false;

        #region NXTStuff

        /// <summary>
        /// Connect to the NXT
        /// </summary>
        /// <param name="ComPort">The COM Port to connect. ex. 4</param>
        public static void ConnectNXT(string ComPort)
        {
            try
            {
                comPort = byte.Parse(ComPort);

                Brick = new NxtBrick(NxtCommLinkType.Bluetooth, comPort);
                NxtMotor motorB = new NxtMotor();
                Brick.MotorB = motorB;
                NxtMotor motorC = new NxtMotor();
                Brick.MotorC = motorC;
                MotorPair = new NxtMotorSync(Brick.MotorB, Brick.MotorC);


                if (!isConnected)
                {
                    Brick.Connect();
                    isConnected = true;
                    Brick.PlaySoundfile("! Attention.rso");
                }
            }
            catch
            {
                isConnected = false;
                DisconnectNXT();
            }
        }

        /// <summary>
        /// Disconnect the NXT
        /// </summary>
        public static void DisconnectNXT()
        {
            Idle();
            Brick.PlaySoundfile("! Attention.rso");
            Brick.CommLink.StopProgram();

            if (Brick != null && Brick.IsConnected)
                isConnected = false;
                Brick.Disconnect();

            Brick = null;
            MotorPair = null;            
        }

        /// <summary>
        /// Idles the Motor Pair
        /// </summary>
        private static void Idle()
        {
            if (Brick != null && Brick.IsConnected)
                MotorPair.Idle();
        }

        /// <summary>
        /// Check NXT Connection
        /// </summary>
        /// <returns></returns>
        public static bool CheckConnection()
        {
            bool ConnectionCheck = false;
            if (Brick != null && Brick.IsConnected)
            {
                ConnectionCheck = true;
            }

            return ConnectionCheck;
        }

        /// <summary>
        /// Get the Current Firmware from the NXT
        /// </summary>
        /// <returns></returns>
        public static string getFirmware()
        {
            if (!isConnected)
            {
                // Connect to the NXT.
                ConnectNXT(ComPort);
            }

            NxtGetFirmwareVersionReply? reply = Brick.CommLink.GetFirmwareVersion();
            if (reply.HasValue)
            {
                NxtGetFirmwareVersionReply nxtreply = (NxtGetFirmwareVersionReply)reply;
                return nxtreply.firmwareVersion.ToString();
            }
            return null;
        }

        /// <summary>
        /// Get the Battery from the NXT
        /// </summary>
        /// <returns></returns>
        public static int getBattery()
        {
            if (!isConnected)
            {
                // Connect to the NXT.
                ConnectNXT(ComPort);
            }

            string[] alex = Brick.Sounds;

            return (Int32)Brick.BatteryLevel;
        }

        /// <summary>
        /// Get the Sounds from the NXT
        /// </summary>
        /// <returns></returns>
        public static string [] getSounds()
        {
            if (!isConnected)
            {
                // Connect to the NXT.
                ConnectNXT(ComPort);
            }

            return  Brick.Sounds;
        }

        /// <summary>
        /// Get the Programs from the NXT
        /// </summary>
        /// <returns></returns>
        public static string[] getPrograms()
        {
            if (!isConnected)
            {
                // Connect to the NXT.
                ConnectNXT(ComPort);
            }

            return Brick.Programs;
        }

        /// <summary>
        /// Show the CU Logo
        /// </summary>
        /// <returns></returns>
        public static void showCULogo()
        {
            if (!isConnected)
            {
                // Connect to the NXT.
                ConnectNXT(ComPort);
            }

            Brick.CommLink.StartProgram("CU.rxe");
        }

        #endregion NXTStuff

        #region R2D2 Navigation

        #region Move Forward

        /// <summary>
        /// Move R2D2 Forward
        /// </summary>
        public static void MoveForward()
        {
            if (isConnected)
            {
                Brick.PlaySoundfile("forward.rso");
                MotorPair.Run(-100, 0, 0);
            }
        }

        #endregion Move Forward

        #region Move Back

        /// <summary>
        /// Move R2D2 Back
        /// </summary>
        public static void MoveBack()
        {
            if (isConnected)
            {
                Brick.PlaySoundfile("forward.rso");
                MotorPair.Run(100, 0, 0);
            }
        }

        #endregion Move Back

        #region Move Left

        /// <summary>
        /// Move R2D2 Left
        /// </summary>
        public static void MoveLeft()
        {
            if (isConnected)
            {
                Brick.PlaySoundfile("left.rso");
                Brick.MotorB.Run(-100, 0);
                Brick.MotorC.Run(100, 0);
            }
        }

        #endregion Move Left

        #region Move Right

        /// <summary>
        /// Move R2D2 Right
        /// </summary>
        public static void MoveRight()
        {
            if (isConnected)
            {
                Brick.PlaySoundfile("right.rso");
                Brick.MotorB.Run(100, 0);
                Brick.MotorC.Run(-100, 0);
            }
        }

        #endregion Move Right

        #region Stop

        /// <summary>
        /// Stop R2D2
        /// </summary>
        public static void Stop()
        {
            if (isConnected)
            {
                Idle();
            }
        }

        #endregion Stop

        #endregion R2D2 Navigation
    }
}
