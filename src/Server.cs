using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client

var responseBuffer = new byte[256]; //buffer to read response from client
int recievedBytes = socket.Receive(responseBuffer); // read response from client

var lines = ASCIIEncoding.UTF8.GetString(responseBuffer).Split("\r\n"); // split response into lines

var line0parts = lines[0].Split(" "); // split first line into words

var (method, path, version) = (line0parts[0], line0parts[1], line0parts[2]); // get method, path and version

var response;
if (path == "/") {
    response = $"HTTP/1.1 200 OK\r\n\r\n"; // check for root path
}else if (path.StartsWith("/echo/")) {
    response = $"HTTP/1.1 200 OK\r\n\r\n{path.Substring(6)}"; // return echo 
} else{
    response = $"HTTP/1.1 404 Not Found\r\n\r\n"; // otherwise return 404
}

socket.Send(Encoding.UTF8.GetBytes(response)); // send response
