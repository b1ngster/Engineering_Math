using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPReceiver
{
    private UdpClient listener;
    private IPEndPoint senderEndPoint;
    private int expectedSequenceNumber = 0;

    public UDPReceiver(int listenPort)
    {
        listener = new UdpClient(listenPort);
        senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
    }

    public void Receive()
    {
        while (true)
        {
            byte[] packet = ReceivePacket();
            int receivedSequenceNumber = BitConverter.ToInt32(packet, 0);
            byte[] data = new byte[packet.Length - sizeof(int)];
            Buffer.BlockCopy(packet, sizeof(int), data, 0, data.Length);

            if (receivedSequenceNumber == expectedSequenceNumber)
            {
                SendAck(expectedSequenceNumber);
                Console.WriteLine("Received message: " + Encoding.UTF8.GetString(data));
                expectedSequenceNumber++;
            }
            else if (receivedSequenceNumber < expectedSequenceNumber)
            {
                SendAck(receivedSequenceNumber); // Acknowledge the duplicate
            }
            else
            {
                // Out-of-order packet received, don't send ACK
            }
        }
    }

    private byte[] ReceivePacket()
    {
        return listener.Receive(ref senderEndPoint);
    }

    private void SendAck(int ackSequenceNumber)
    {
        byte[] ackData = BitConverter.GetBytes(ackSequenceNumber);
        listener.Send(ackData, ackData.Length, senderEndPoint);
    }
}