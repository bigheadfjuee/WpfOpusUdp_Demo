using System;
using System.Windows;
using System.Windows.Forms;
using NAudio.Wave;
using OpusDotNet;

namespace WpfOpusUdp
{
   /// <summary>
   /// MainWindow.xaml 的互動邏輯
   /// </summary>
   public partial class MainWindow : Window
   {
      MyUdpClient myUdp;
      public MainWindow()
      {
         InitializeComponent();
         myUdp = new MyUdpClient("192.168.2.7", 5566);
         btnStart.IsEnabled = true;
         btnStop.IsEnabled = false;
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         for (int i = 0; i < WaveIn.DeviceCount; i++)
         {
            comboBoxIn.Items.Add(WaveIn.GetCapabilities(i).ProductName);
         }
         if (WaveIn.DeviceCount > 0)
            comboBoxIn.SelectedIndex = 0;
         for (int i = 0; i < WaveOut.DeviceCount; i++)
         {
            comboBoxOut.Items.Add(WaveOut.GetCapabilities(i).ProductName);
         }
         if (WaveOut.DeviceCount > 0)
            comboBoxOut.SelectedIndex = 0;
      }

      [Obsolete]
      private void btnStart_Click(object sender, RoutedEventArgs e)
      {
         Console.Write("start");
         StartEncoding();
         btnStart.IsEnabled = false;
         btnStop.IsEnabled = true;

      }

      private void btnStop_Click(object sender, RoutedEventArgs e)
      {
         Console.Write("stop");
         StopEncoding();
         btnStart.IsEnabled = true;
         btnStop.IsEnabled = false;
      }


      WaveIn _waveIn;
      WaveOut _waveOut;
      BufferedWaveProvider _playBuffer;
      OpusEncoder _encoder;
      OpusDecoder _decoder;
      
      ulong _bytesSent;
      DateTime _startTime;
      Timer _timer = null;

      [Obsolete]
      void StartEncoding()
      {
         _startTime = DateTime.Now;
         _bytesSent = 0;

         int ch = 2;

         _encoder = new OpusEncoder(OpusDotNet.Application.VoIP, 48000, ch);
         _decoder = new OpusDecoder(48000, ch);

         _waveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
         _waveIn.BufferMilliseconds = 50;
         _waveIn.DeviceNumber = comboBoxIn.SelectedIndex;
         _waveIn.DataAvailable += _waveIn_DataAvailable;
         _waveIn.WaveFormat = new WaveFormat(48000, 16, ch);

         _playBuffer = new BufferedWaveProvider(new WaveFormat(48000, 16, ch));

         _waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());
         _waveOut.DeviceNumber = comboBoxOut.SelectedIndex;
         _waveOut.Init(_playBuffer);

         _waveOut.Play();
         _waveIn.StartRecording();

         if (_timer == null)
         {
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += _timer_Tick;
         }
         _timer.Start();
      }

      void _timer_Tick(object sender, EventArgs e)
      {
         var timeDiff = DateTime.Now - _startTime;
         var bytesPerSecond = _bytesSent / timeDiff.TotalSeconds;
         Console.WriteLine("{0} Bps", bytesPerSecond);
      }

      byte[] _notEncodedBuffer = new byte[0];

      [Obsolete]
      void _waveIn_DataAvailable(object sender, WaveInEventArgs e)
      {
         byte[] soundBuffer = new byte[e.BytesRecorded + _notEncodedBuffer.Length];
         for (int i = 0; i < _notEncodedBuffer.Length; i++)
            soundBuffer[i] = _notEncodedBuffer[i];
         for (int i = 0; i < e.BytesRecorded; i++)
            soundBuffer[i + _notEncodedBuffer.Length] = e.Buffer[i];

         int byteCap = 960;
         int segmentCount = (int)Math.Floor((decimal)soundBuffer.Length / byteCap);
         int segmentsEnd = segmentCount * byteCap;
         int notEncodedCount = soundBuffer.Length - segmentsEnd;
         _notEncodedBuffer = new byte[notEncodedCount];
         for (int i = 0; i < notEncodedCount; i++)
         {
            _notEncodedBuffer[i] = soundBuffer[segmentsEnd + i];
         }

         for (int i = 0; i < segmentCount; i++)
         {
            byte[] segment = new byte[byteCap];
            for (int j = 0; j < segment.Length; j++)
               segment[j] = soundBuffer[(i * byteCap) + j];
            int len;
            byte[] buff = _encoder.Encode(segment, segment.Length, out len);
            _bytesSent += (ulong)len;

            myUdp.SendBuffer(buff);

            buff = _decoder.Decode(buff, len, out len);
            _playBuffer.AddSamples(buff, 0, len);

         }
      }

      void StopEncoding()
      {
         _timer.Stop();
         _waveIn.StopRecording();
         _waveIn.Dispose();
         _waveIn = null;
         _waveOut.Stop();
         _waveOut.Dispose();
         _waveOut = null;
         _playBuffer = null;
         _encoder.Dispose();
         _encoder = null;
         _decoder.Dispose();
         _decoder = null;
      }


   }
}
