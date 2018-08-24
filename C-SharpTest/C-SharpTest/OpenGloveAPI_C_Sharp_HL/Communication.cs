﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using WebSocketSharp; //Nuget: WebSocketSharp-netstandard (1.0.1)

namespace CSharpTest.OpenGloveAPI_C_Sharp_HL
{
    public class Communication
    {
        public WebSocket WebSocket { get; set; }
        public MessageGenerator MessageGenerator { get; set; }
        public string BluetoothDeviceName { get; set; }
        private bool _IsConnectedToBluetoothDevice { get; set; }


        public delegate void FlexorMovement(int region, int value);
        public delegate void AccelerometerValues(float ax, float ay, float az);
        public delegate void GyroscopeValues(float gx, float gy, float gz);
        public delegate void MagnometerValues(float mx, float my, float mz);
        public delegate void AllIMUValues(float ax, float ay, float az, float gx, float gy, float gz, float mx, float my, float mz);
        public delegate void BluetoothDeviceState(bool isConnected);
        public delegate void InfoMessage(string message);

        public event FlexorMovement OnFlexorValueReceived;
        public event AccelerometerValues OnAccelerometerValuesReceived;
        public event GyroscopeValues OnGyroscopeValuesReceived;
        public event MagnometerValues OnMagnometerValuesReceived;
        public event AllIMUValues OnAllIMUValuesReceived;
        public event BluetoothDeviceState OnBluetoothDeviceStateChanged;
        public event InfoMessage OnInfoMessagesReceived;

        public bool IsConnectedToBluetoohDevice { get { return _IsConnectedToBluetoothDevice; } }
        public Communication(string bluetoothDeviceName, string url)
        {
            WebSocket = new WebSocket(url);
            WebSocket.OnOpen += OnOpen;         
            WebSocket.OnMessage += OnMessage;
            WebSocket.OnClose += OnClose;
            WebSocket.OnError += OnError;

            _IsConnectedToBluetoothDevice = false;
            BluetoothDeviceName = bluetoothDeviceName;

            MessageGenerator = new MessageGenerator(mainSeparator: ";", secondarySeparator: ",", empty: "");
        }

        private void OnOpen(object sender, EventArgs e)
        {
            Debug.WriteLine($"Websocket Connected to Server!");
            this.AddOpenGloveDeviceToServer(BluetoothDeviceName);
            Debug.WriteLine($"OpenGlove.Communication.AddOpenGloveToServer()");
            this.StartCaptureDataFromServer(BluetoothDeviceName);
            Debug.WriteLine($"Communication.StartCaptureDataFromServer()");

        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            Debug.WriteLine($"Received from Server: {e.Data}");
            MessageHandler(e.Data);
        }

        private void MessageHandler(string message)
        {
            int mapping, value;
            float valueX, valueY, valueZ;
            string[] words;

            if (message != null)
            {
                words = message.Split(',');
                try
                {
                    switch (words[0])
                    {
                        case "f":
                            mapping = Int32.Parse(words[1]);
                            value = Int32.Parse(words[2]);
                            OnFlexorValueReceived?.Invoke(mapping, value);
                            break;
                        case "a":
                            valueX = float.Parse(words[1], CultureInfo.InvariantCulture);
                            valueY = float.Parse(words[2], CultureInfo.InvariantCulture);
                            valueZ = float.Parse(words[3], CultureInfo.InvariantCulture);
                            OnAccelerometerValuesReceived?.Invoke(valueX, valueY, valueZ);
                            break;
                        case "g":
                            valueX = float.Parse(words[1], CultureInfo.InvariantCulture);
                            valueY = float.Parse(words[2], CultureInfo.InvariantCulture);
                            valueZ = float.Parse(words[3], CultureInfo.InvariantCulture);
                            OnGyroscopeValuesReceived?.Invoke(valueX, valueY, valueZ);
                            break;
                        case "m":
                            valueX = float.Parse(words[1], CultureInfo.InvariantCulture);
                            valueY = float.Parse(words[2], CultureInfo.InvariantCulture);
                            valueZ = float.Parse(words[3], CultureInfo.InvariantCulture);
                            OnMagnometerValuesReceived?.Invoke(valueX, valueY, valueZ);
                            break;
                        case "z":
                            OnAllIMUValuesReceived?.Invoke(float.Parse(words[1], CultureInfo.InvariantCulture), float.Parse(words[2], CultureInfo.InvariantCulture), float.Parse(words[3], CultureInfo.InvariantCulture), float.Parse(words[4], CultureInfo.InvariantCulture), float.Parse(words[5], CultureInfo.InvariantCulture), float.Parse(words[6], CultureInfo.InvariantCulture), float.Parse(words[7], CultureInfo.InvariantCulture), float.Parse(words[8], CultureInfo.InvariantCulture), float.Parse(words[9], CultureInfo.InvariantCulture));
                            break;
                        case "b":
                            OnBluetoothDeviceStateChanged?.Invoke(bool.Parse(words[1]));
                            break;
                            
                        default:
                            OnInfoMessagesReceived?.Invoke(message);
                            break;
                    }

                }
                catch
                {
                    Console.WriteLine("ERROR: BAD FORMAT");
                }
            }
        }

        private void OnClose(object sender, EventArgs e)
        {
            Debug.WriteLine($"WebSocket Closed: {e.ToString()}");
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine($"WebSocket Error: {e.Exception}, {e.ToString()}");
        }

        public void StartOpenGlove(string bluetoothDeviceName, string configurationName)
        {
            this.WebSocket.Send(MessageGenerator.StartOpenGlove(bluetoothDeviceName, configurationName));
        }

        public void StopOpenGlove(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.StopOpenGlove(bluetoothDeviceName));
        }

        public void AddOpenGloveDeviceToServer(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.AddOpenGloveDeviceToServer(bluetoothDeviceName));
        }

        public void RemoveOpenGloveDeviceFromServer(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.RemoveOpenGloveDeviceFromServer(bluetoothDeviceName));
        }
        public void SaveOpenGloveConfiguration(string bluetoothDeviceName, string configurationName)
        {
            this.WebSocket.Send(MessageGenerator.SaveOpenGloveConfiguration(bluetoothDeviceName, configurationName));
        }

        public void ConnectToBluetoothDevice(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.ConnectToBluetoothDevice(bluetoothDeviceName));
        }

        public void DisconnectFromBluetoothDevice(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.DisconnectFromBluetoothDevice(bluetoothDeviceName));
        }

        public void StartCaptureDataFromServer(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.StartCaptureDataFromServer(bluetoothDeviceName));
        }

        public void StopCaptureDataFromServer(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.StopCaptureDataFromServer(bluetoothDeviceName));
        }

        public void AddActuator(string bluetoothDeviceName, int region, int positivePin, int negativePin)
        {
            this.WebSocket.Send(MessageGenerator.AddActuator(bluetoothDeviceName, region, positivePin, negativePin));
        }

        public void AddActuators(string bluetoothDeviceName, List<int> regions, List<int> positivePins, List<int> negativePins)
        {
            this.WebSocket.Send(MessageGenerator.AddActuators(bluetoothDeviceName, regions, positivePins, negativePins));
        }

        public void RemoveActuator(string bluetoothDeviceName, int region)
        {
            this.WebSocket.Send(MessageGenerator.RemoveActuator(bluetoothDeviceName, region));
        }

        public void RemoveActuators(string bluetoothDeviceName, List<int> regions)
        {
            this.WebSocket.Send(MessageGenerator.RemoveActuators(bluetoothDeviceName, regions));
        }

        public void ActivateActuators(string bluetoothDeviceName, List<int> regions, List<string> intensities)
        {
            this.WebSocket.Send(MessageGenerator.ActivateActuators(bluetoothDeviceName, regions, intensities));
        }

        public void TurnOnActuators(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.TurnOnActuators(bluetoothDeviceName));
        }

        public void TurnOffActuators(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.TurnOffActuators(bluetoothDeviceName));
        }

        public void ResetActuators(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.ResetActuators(bluetoothDeviceName));
        }

        public void AddFlexor(string bluetoothDeviceName, int region, int pin)
        {
            this.WebSocket.Send(MessageGenerator.AddFlexor(bluetoothDeviceName, region, pin));
        }

        public void AddFlexors(string bluetoothDeviceName, List<int> regions, List<int> pins)
        {
            this.WebSocket.Send(MessageGenerator.AddFlexors(bluetoothDeviceName, regions, pins));
        }

        public void RemoveFlexor(string bluetoothDeviceName, int region)
        {
            this.WebSocket.Send(MessageGenerator.RemoveFlexor(bluetoothDeviceName, region));
        }

        public void RemoveFlexors(string bluetoothDeviceName, List<int> regions)
        {
            this.WebSocket.Send(MessageGenerator.RemoveFlexors(bluetoothDeviceName, regions));
        }

        public void CalibrateFlexors(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.CalibrateFlexors(bluetoothDeviceName));
        }

        public void ConfirmCalibration(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.ConfirmCalibration(bluetoothDeviceName));
        }

        public void SetThreshold(string bluetoothDeviceName, int value)
        {
            this.WebSocket.Send(MessageGenerator.SetThreshold(bluetoothDeviceName, value));
        }

        public void TurnOnFlexors(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.TurnOnFlexors(bluetoothDeviceName));
        }

        public void TurnOffFlexors(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.TurnOffFlexors(bluetoothDeviceName));
        }

        public void ResetFlexors(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.ResetFlexors(bluetoothDeviceName));
        }

        public void StartIMU(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.StartIMU(bluetoothDeviceName));
        }

        public void SetIMUStatus(string bluetoothDeviceName, bool status)
        {
            this.WebSocket.Send(MessageGenerator.SetIMUStatus(bluetoothDeviceName, status));
        }

        public void SetRawData(string bluetoothDeviceName, bool status)
        {
            this.WebSocket.Send(MessageGenerator.SetRawData(bluetoothDeviceName, status));
        }

        public void SetIMUChoosingData(string bluetoothDeviceName, int value)
        {
            this.WebSocket.Send(MessageGenerator.SetIMUChoosingData(bluetoothDeviceName, value));
        }

        public void CalibrateIMU(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.CalibrateIMU(bluetoothDeviceName));
        }

        public void SetLoopDelay(string bluetoothDeviceName, int value)
        {
            this.WebSocket.Send(MessageGenerator.SetLoopDelay(bluetoothDeviceName, value));
        }

        public void ConnectToWebSocketServer()
        {
            this.WebSocket.Connect();
        }

        public void DisconnectFromWebSocketServer()
        {
            this.WebSocket.Close();
        }

        public void GetOpenGloveArduinoVersionSoftware(string bluetoothDeviceName)
        {
            this.WebSocket.Send(MessageGenerator.GetOpenGloveArduinoVersionSoftware(bluetoothDeviceName));
        }
    }
}
