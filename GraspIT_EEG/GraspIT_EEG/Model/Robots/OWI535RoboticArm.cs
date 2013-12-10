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
 * Description
 * - OWI 535 Robotic Arm Control Library
  */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraspIT_EEG.Properties;
using System.Threading;

// OWI 535 Robotic Arm Library
using OWI535.Library;

namespace GraspIT_EEG.Model.Robots
{
    public static class OWI535RoboticArm
    {
        public static readonly int VendorID = Settings.Default.VendorID;
        public static readonly int ProductID = Settings.Default.ProductID;
        public static readonly int ArmID = Settings.Default.ArmID;
        public static ArmController Arm = new ArmController(VendorID, ProductID, ArmID);
        public static bool isArmConnected = false;

        public static Thread[] ArmThreadArray = new Thread[1];

        #region  Arm

        /// <summary>
        /// Returns the arm seconds
        /// </summary>
        /// <param name="ArmSecondsStringValue">The arm seconds string value.</param>
        /// <returns></returns>
        public static int getArmSeconds(string ArmSecondsStringValue)
        {
            int seconds;
            Int32.TryParse(ArmSecondsStringValue, out seconds);
            return (seconds * 1000);
        }

        /// <summary>
        /// Turns the light On
        /// </summary>
        public static void LightsOn()
        {
                ArmThreadArray[0] = new Thread(() => Arm.LightOn());
                ArmThreadArray[0].Start();           
        }

        /// <summary>
        /// Turns the light Off
        /// </summary>
        public static void LightsOff()
        {
            ArmThreadArray[0] = new Thread(() => Arm.LightOff());
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Opens the Grippers
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void GrippersOpen(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ClawOpen(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Closes the Grippers
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void GrippersClose(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ClawClose(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Grippers Up
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void GrippersUp(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ClawUp(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Grippers Down
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void GrippersDown(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ClawDown(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Elbow Up
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ElbowUp(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ElbowUp(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Elbow Down
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ElbowDown(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ElbowDown(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Arm Up
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ArmUp(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ArmUp(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Moves the Arm Down
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ArmDown(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.ArmDown(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Rotates the Arm Left
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ArmRotateLeft(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.RotateLeft(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Rotates the Arm Right
        /// </summary>
        /// <param name="ArmSeconds">The Arms Seconds</param>
        public static void ArmRotateRight(int ArmSeconds)
        {
            ArmThreadArray[0] = new Thread(() => Arm.RotateRight(ArmSeconds));
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Performs Handshake gesture
        /// </summary>
        public static void Handshake()
        {
            ArmThreadArray[0] = new Thread(() => Arm.Handshake());
            ArmThreadArray[0].Start();
        }

        /// <summary>
        /// Stops the Arm
        /// </summary>
        public static void ArmStop()
        {
            if (ArmThreadArray[0] != null)
            {
                ArmThreadArray[0].Abort();
                Arm.Reset();
            }
        }

        public static void EyeFlash(int times)
        {
            ArmThreadArray[0] = new Thread(() => Arm.FlashEye(times));
            ArmThreadArray[0].Start();
        }

        #endregion  Arm
    }
}
