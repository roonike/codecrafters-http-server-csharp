using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    TcpClient client = server.AcceptTcpClient(); // wait for client
    NetworkStream stream = client.GetStream();
    
    var responseBuffer = new byte[256]; //buffer to read response from client
    int recievedBytes = stream.Read(responseBuffer, 0, responseBuffer.Length); // read response from client

    string request = Encoding.UTF8.GetString(responseBuffer, 0, recievedBytes); // convert byte array to string

    var lines = request.Split(new string { "\r\n" }, StringSplitOptions.None); // split request into lines

    string line0 = lines[0]; // get first line

    var (method, path, version) = line0.Split(" "); // split first line into method, path and version

    string response;

    if (path == "/") {
        response = $"HTTP/1.1 200 OK\r\n\r\n"; // check for root path
    } else if (path.StartsWith("/echo/")) {
        response = $"HTTP/1.1 200 OK\r\n\r\n{path.Substring(6)}"; // return echo 
    } else{
        response = $"HTTP/1.1 404 Not Found\r\n\r\n"; // otherwise return 404
    }

    byte[] responseBytes = Encoding.ASCII.GetBytes(response); // convert string to byte array
    stream.Write(responseBytes, 0, responseBytes.Length); // send response

    client.Close(); // close client
}