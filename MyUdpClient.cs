using System;
using System.Net.Sockets;

namespace WpfOpusUdp
{
   public class MyUdpClient
   {
      string Host = "";
      int RemotePort = 0;

      private UdpClient udpClient = null;

      public MyUdpClient(string host, int remotePort)
      {
         udpClient = new UdpClient();
         this.Host = host;
         this.RemotePort = remotePort;
      }

      public async void SendBuffer(byte[] buffer)
      {
         if (udpClient == null) return;
         if (buffer.Length == 0) return;

         try
         {
            int len = await udpClient.SendAsync(buffer, buffer.Length,
              this.Host, this.RemotePort);

            //Console.Write(len);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }
      }

      public void Close()
      {
         if (udpClient == null)
            return;

         try
         {
            udpClient.Client.Shutdown(SocketShutdown.Both);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         udpClient.Close();
         udpClient = null;
      }
   }

}
