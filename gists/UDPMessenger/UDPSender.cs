using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UDPSender
{
    private UdpClient client;
    private IPEndPoint serverEndPoint;
    private int sequenceNumber = 0;

    public UDPSender(string serverIp, int serverPort)
    {
        client = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
    }

    public void Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        byte[] packet = CreatePacket(data);

        while (true)
        {
            SendPacket(packet);

            // Wait for acknowledgment (ACK)
            byte[] ackData = ReceiveAck();
            int ackSequenceNumber = BitConverter.ToInt32(ackData, 0);

            if (ackSequenceNumber == sequenceNumber)
            {
                Console.WriteLine("Message sent and acknowledged: " + message);
                sequenceNumber++;
                break;
            }
        }
    }

    private byte[] CreatePacket(byte[] data)
    {
        byte[] sequenceBytes = BitConverter.GetBytes(sequenceNumber);
        byte[] packet = new byte[sequenceBytes.Length + data.Length];

        Buffer.BlockCopy(sequenceBytes, 0, packet, 0, sequenceBytes.Length);
        Buffer.BlockCopy(data, 0, packet, sequenceBytes.Length, data.Length);

        return packet;
    }

    private void SendPacket(byte[] packet)
    {
        client.Send(packet, packet.Length, serverEndPoint);
    }

    private byte[] ReceiveAck()
    {
        return client.Receive(ref serverEndPoint);
    }
}