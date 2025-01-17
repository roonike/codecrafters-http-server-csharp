using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.IO.Compression;

TcpListener server = new TcpListener(IPAddress.Any, 4221); // create server
server.Start(); // start server

string[] ValidEncoders = ["gzip"]; // list of valid encoders  

while (true) { 
    TcpClient client = server.AcceptTcpClient(); // wait for client to connect
    NetworkStream stream = client.GetStream(); // get client stream 
    
    var responseBuffer = new byte[256]; //buffer to read response from client
    int recievedBytes = stream.Read(responseBuffer, 0, responseBuffer.Length); // read response from client into buffer

    string request = Encoding.UTF8.GetString(responseBuffer, 0, recievedBytes); // convert byte array to string 

    string[] lines = request.Split("\r\n"); // split request into lines 

    string[] startLineParts = lines[0].Split(' '); // split first line into method, path and version 

    string response; // variable to store response string

    string? encoding = null;
        foreach (string line in lines) {
            if (line.StartsWith("Accept-Encoding:")) {
                if (line.Contains("gzip")) {
                    encoding = "gzip";
                    break;
                }
        }
  }

    if (startLineParts[1] == "/") {
        response = $"HTTP/1.1 200 OK\r\n\r\n"; // check for root path
    } else if (startLineParts[1].StartsWith("/echo/")) {
        byte[] compressedResponse = [];
    if (encoding == "gzip") {
      byte[] messageBytes = Encoding.UTF8.GetBytes(startLineParts[1].Split('/')[2]);
      using (var outputStream = new MemoryStream()) {
        using (var gZipStream =
                   new GZipStream(outputStream, CompressionMode.Compress)) {
          gZipStream.Write(messageBytes, 0, messageBytes.Length);
        }
        compressedResponse = outputStream.ToArray();
      }
    } else {
      compressedResponse = Encoding.UTF8.GetBytes(startLineParts[1].Split('/')[2]);
    }
    encoding = encoding != null 
                ? $"\r\nContent-Encoding: {encoding}" 
                : "";
    var compressedMessage =
        $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {compressedResponse.Length}{encoding}\r\n\r\n";
    byte[] responseEchoBytes =
        [..Encoding.UTF8.GetBytes(compressedMessage), ..compressedResponse];
    stream.Write(responseEchoBytes, 0, responseEchoBytes.Length);
    continue;
    } else if (startLineParts[1].StartsWith("/user-agent")) {
        string userAgent = lines[2].Split(' ')[1];// get User-Agent
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n{userAgent}"; // return User-Agent
    } else if (startLineParts[1].StartsWith("/files/") && startLineParts[0] == "GET") {
        var directory = Environment.GetCommandLineArgs()[2]; // get directory from command line
        string fileName = startLineParts[1].Split("/")[2]; // get file name from path with form abc/files/fileName
        string filePath = $"{directory}/{fileName}"; // create file path
        // read file contents
        if (File.Exists(filePath)) { // check if file exists
            string fileContents = File.ReadAllText(filePath); // read file
            response = $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContents.Length}\r\n\r\n{fileContents}";
        } else { // otherwise return 404
            response = $"HTTP/1.1 404 Not Found\r\n\r\n"; 
        }
    }else if (startLineParts[1].StartsWith("/files/") && startLineParts[0] == "POST") { // POST request for files
        var directory = Environment.GetCommandLineArgs()[2]; // get directory from command line
        string fileName = startLineParts[1].Split("/")[2]; // get filename from path
        string filePath = $"{directory}/{fileName}"; // create file path
        
        // write to file
        string fileContents = lines[lines.Length - 1];
        File.WriteAllText(filePath, fileContents); // write to file
        response = $"HTTP/1.1 201 Created\r\n\r\n"; // return success response
    } else{ // 404 for every non handled request
        response = $"HTTP/1.1 404 Not Found\r\n\r\n"; // otherwise return 404
    }

    byte[] responseBytes = Encoding.ASCII.GetBytes(response); // convert string to byte array
    stream.Write(responseBytes, 0, responseBytes.Length); // send response

    client.Close(); // close client
}