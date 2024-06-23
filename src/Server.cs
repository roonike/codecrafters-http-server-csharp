using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;


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
    string[]? encoders = null;
        foreach (string line in lines) {
            if (line.StartsWith("Accept-Encoding:")) {
                string[] vals = line.split(" ");
                encoding = vals[1];
                encoders.Add(ValidEncoders[1, vals.Length - 1]);
                break;
            }
        }
  

    if (startLineParts[1] == "/") {
        response = $"HTTP/1.1 200 OK\r\n\r\n"; // check for root path
    } else if (startLineParts[1].StartsWith("/echo/")) {
        if (encoders.Length > 1){   

            foreach (string encode in encoders){
                if (encode == "gzip"){
                    response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Encoding: gzip\r\nContent-Length: {message.Length}\r\n\r\n{message}";
                }
            }
        }else{
        encoding = encoding != null && ValidEncoders.Contains(encoding) // check if encoding is valid
                    ? $"\r\nContent-Encoding: {encoding}" // add encoding header
                    : ""; // if not valid, do not add header
        string message = startLineParts[1].Substring(6); // get message from path
        response = $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {message.Length}{encoding}\r\n\r\n{message}"; // return echo 
        }

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