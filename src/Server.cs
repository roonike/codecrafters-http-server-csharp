using System.Net;
using System.Net.Sockets;
using system.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client
socket.send("HTTP/1.1 200 OK\r\n\r\n");
