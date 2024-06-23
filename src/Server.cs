using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;


TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();

while (true) {
    TcpClient client = server.AcceptTcpClient(); // wait for client
    NetworkStream stream = client.GetStream(); // get client stream
    
    var responseBuffer = new byte[256]; //buffer to read response from client
    int recievedBytes = stream.Read(responseBuffer, 0, responseBuffer.Length); // read response from client

    string request = Encoding.UTF8.GetString(responseBuffer, 0, recievedBytes); // convert byte array to string

    string[] lines = request.Split("\r\n"); // split request into lines

    string[] startLineParts = lines[0].Split(' '); // split first line into method, path and version

    string response; // variable to store response

    if (startLineParts[1] == "/") {
        response = $"HTTP/1.1 200 OK\r\n\r\n"; // check for root path
    } else if (startLineParts[1].StartsWith("/echo/")) {
        string message = startLineParts[1].Substring(6); // get message from path
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {message.Length}\r\n\r\n{message}"; // return echo 
    } else if (startLineParts[1].StartsWith("/user-agent")) {
        string userAgent = lines[2].Split(' ')[1];// get User-Agent
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n{userAgent}"; // return User-Agent
    } else if (startLineParts[1].StartsWith("/files/")) {
        var directory = Environment.GetCommandLineArgs()[2];
        var fileName = startLineParts[1].Split("/")[2];
        var filePath = $"{directory}/{fileName}";
        // read file contents
        if (File.Exists(filePath)) { // check if file exists
            string fileContents = File.ReadAllText(filePath); // read file
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContents.Length}\r\n\r\n{fileContents}";
        } else { // otherwise return 404
            response = $"HTTP/1.1 404 Not Found\r\n\r\n"; 
        }
    } else{
        response = $"HTTP/1.1 404 Not Found\r\n\r\n"; // otherwise return 404
    }

    byte[] responseBytes = Encoding.ASCII.GetBytes(response); // convert string to byte array
    stream.Write(responseBytes, 0, responseBytes.Length); // send response

    client.Close(); // close client
}