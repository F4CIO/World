﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using CBB_LoggingCustomTraceLog = CraftSynth.BuildingBlocks.Logging.CustomTraceLog;
using CBB_LoggingCustomTraceLogExtensions = CraftSynth.BuildingBlocks.Logging.CustomTraceLogExtensions;

namespace CraftSynth.BuildingBlocks.IO
{
    class FtpClient
    {
		

        private string remoteHost, remotePath, remoteUser, remotePass, mes;
        private int remotePort, bytes;
        private Socket clientSocket;

        private int retValue;
        private Boolean debug;
        private Boolean logined;
        private string reply;

        private static int BLOCK_SIZE = 512;

        Byte[] buffer = new Byte[BLOCK_SIZE];
        Encoding ASCII = Encoding.ASCII;

        public FtpClient()
        {

            remoteHost = "localhost";
            remotePath = ".";
            remoteUser = "anonymous";
            remotePass = "";
            remotePort = 21;
            debug = false;
            logined = false;

        }

        ///
        /// Set the name of the FTP server to connect to.
        ///
        /// Server name
        public void setRemoteHost(string remoteHost)
        {
            this.remoteHost = remoteHost;
        }

        ///
        /// Return the name of the current FTP server.
        ///
        /// Server name
        public string getRemoteHost()
        {
            return remoteHost;
        }

        ///
        /// Set the port number to use for FTP.
        ///
        /// Port number
        public void setRemotePort(int remotePort)
        {
            this.remotePort = remotePort;
        }

        ///
        /// Return the current port number.
        ///
        /// Current port number
        public int getRemotePort()
        {
            return remotePort;
        }

        ///
        /// Set the remote directory path.
        ///
        /// The remote directory path
        public void setRemotePath(string remotePath)
        {
            this.remotePath = remotePath;
        }

        ///
        /// Return the current remote directory path.
        ///
        /// The current remote directory path.
        public string getRemotePath()
        {
            return remotePath;
        }

        ///
        /// Set the user name to use for logging into the remote server.
        ///
        /// Username
        public void setRemoteUser(string remoteUser)
        {
            this.remoteUser = remoteUser;
        }

        ///
        /// Set the password to user for logging into the remote server.
        ///
        /// Password
        public void setRemotePass(string remotePass)
        {
            this.remotePass = remotePass;
        }

        ///
        /// Return a string array containing the remote directory's file list.
        ///
        ///
        ///
        public string[] getFileList(string mask, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            Socket cSocket = createDataSocket();

            sendCommand("NLST " + mask);

            if (!(retValue == 150 || retValue == 125))
            {
                throw new IOException(reply.Substring(4));
            }

            mes = "";

            while (true)
            {

                int bytes = cSocket.Receive(buffer, buffer.Length, 0);
                mes += ASCII.GetString(buffer, 0, bytes);

                if (bytes < buffer.Length)
                {
                    break;
                }
            }

            char[] seperator = { '\n' };
            string[] mess = mes.Split(seperator);

            cSocket.Close();

            readReply();

            if (retValue != 226)
            {
                throw new IOException(reply.Substring(4));
            }
            return mess;

        }

        ///
        /// Return the size of a file.
        ///
        ///
        ///
        public long getFileSize(string fileName, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            sendCommand("SIZE " + fileName);
            long size = 0;

            if (retValue == 213)
            {
                size = Int64.Parse(reply.Substring(4));
            }
            else
            {
                throw new IOException(reply.Substring(4));
            }

            return size;

        }

        ///
        /// Login to the remote server.
        ///
        public void login(CBB_LoggingCustomTraceLog log)
        {

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(Dns.Resolve(remoteHost).AddressList[0], remotePort);

            try
            {
                clientSocket.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("Couldn't connect to remote server");
            }

            System.Threading.Thread.Sleep(1000);
            readReply();
            if (retValue != 220)
            {
                close(log);
                throw new IOException(reply.Substring(4));
            }
            if (debug)
                CBB_LoggingCustomTraceLogExtensions.AddLine(log, "USER " + remoteUser);

            sendCommand("USER " + remoteUser);

            if (!(retValue == 331 || retValue == 230))
            {
                cleanup();
                throw new IOException(reply.Substring(4));
            }

            if (retValue != 230)
            {
                if (debug)
					CBB_LoggingCustomTraceLogExtensions.AddLine(log, "PASS xxx");

                sendCommand("PASS " + remotePass);
                if (!(retValue == 230 || retValue == 202))
                {
                    cleanup();
                    throw new IOException(reply.Substring(4));
                }
            }

            logined = true;
			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Connected to " + remoteHost);

            chdir(remotePath, log);

        }

        ///
        /// If the value of mode is true, set binary mode for downloads.
        /// Else, set Ascii mode.
        ///
        ///
        public void setBinaryMode(Boolean mode)
        {

            if (mode)
            {
                sendCommand("TYPE I");
            }
            else
            {
                sendCommand("TYPE A");
            }
            if (retValue != 200)
            {
                throw new IOException(reply.Substring(4));
            }
        }

        ///
        /// Download a file to the Assembly's local directory,
        /// keeping the same file name.
        ///
        ///
        public void download(string remFileName, CBB_LoggingCustomTraceLog log)
        {
            download(remFileName, "", false, log);
        }

        ///
        /// Download a remote file to the Assembly's local directory,
        /// keeping the same file name, and set the resume flag.
        ///
        ///
        ///
        public void download(string remFileName, Boolean resume, CBB_LoggingCustomTraceLog log)
        {
            download(remFileName, "", resume, log);
        }

        ///
        /// Download a remote file to a local file name which can include
        /// a path. The local file name will be created or overwritten,
        /// but the path must exist.
        ///
        ///
        ///
        public void download(string remFileName, string locFileName, CBB_LoggingCustomTraceLog log)
        {
            download(remFileName, locFileName, false, log);
        }

        ///
        /// Download a remote file to a local file name which can include
        /// a path, and set the resume flag. The local file name will be
        /// created or overwritten, but the path must exist.
        ///
        ///
        ///
        ///
        public void download(string remFileName, string locFileName, Boolean resume, CBB_LoggingCustomTraceLog log)
        {
            if (!logined)
            {
                login(log);
            }

            setBinaryMode(true);

			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Downloading file " + remFileName + " from " + remoteHost + "/" + remotePath);

            if (locFileName.Equals(""))
            {
                locFileName = remFileName;
            }

            if (!File.Exists(locFileName))
            {
                Stream st = File.Create(locFileName);
                st.Close();
            }

            FileStream output = new FileStream(locFileName, FileMode.Open);

            Socket cSocket = createDataSocket();

            long offset = 0;

            if (resume)
            {

                offset = output.Length;

                if (offset > 0)
                {
                    sendCommand("REST " + offset);
                    if (retValue != 350)
                    {
                        //throw new IOException(reply.Substring(4));
                        //Some servers may not support resuming.
                        offset = 0;
                    }
                }

                if (offset > 0)
                {
                    if (debug)
                    {
						CBB_LoggingCustomTraceLogExtensions.AddLine(log, "seeking to " + offset);
                    }
                    long npos = output.Seek(offset, SeekOrigin.Begin);
					CBB_LoggingCustomTraceLogExtensions.AddLine(log, "new pos=" + npos);
                }
            }

            sendCommand("RETR " + remFileName);

            if (!(retValue == 150 || retValue == 125))
            {
                throw new IOException(reply.Substring(4));
            }

            while (true)
            {

                bytes = cSocket.Receive(buffer, buffer.Length, 0);
                output.Write(buffer, 0, bytes);

                if (bytes <= 0)
                {
                    break;
                }
            }

            output.Close();
            if (cSocket.Connected)
            {
                cSocket.Close();
            }

            Console.WriteLine("");

            readReply();

            if (!(retValue == 226 || retValue == 250))
            {
                throw new IOException(reply.Substring(4));
            }

        }

        ///
        /// Upload a file.
        ///
        ///
        public void upload(string fileName, CBB_LoggingCustomTraceLog log)
        {
            upload(fileName, false, log);
        }

        ///
        /// Upload a file and set the resume flag.
        ///
        ///
        ///
        public void upload(string fileName, Boolean resume, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            Socket cSocket = createDataSocket();
            long offset = 0;

            if (resume)
            {

                try
                {

                    setBinaryMode(true);
                    offset = getFileSize(fileName, log);

                }
                catch (Exception)
                {
                    offset = 0;
                }
            }

            if (offset > 0)
            {
                sendCommand("REST " + offset);
                if (retValue != 350)
                {
                    //throw new IOException(reply.Substring(4));
                    //Remote server may not support resuming.
                    offset = 0;
                }
            }

            sendCommand("STOR " + Path.GetFileName(fileName));

            if (!(retValue == 125 || retValue == 150))
            {
                throw new IOException(reply.Substring(4));
            }

            // open input stream to read source file
            FileStream input = new FileStream(fileName, FileMode.Open);

            if (offset != 0)
            {

                if (debug)
                {
					CBB_LoggingCustomTraceLogExtensions.AddLine(log, "seeking to " + offset);
                }
                input.Seek(offset, SeekOrigin.Begin);
            }

			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Uploading file " + fileName + " to " + remotePath);
			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Progress:");
            while ((bytes = input.Read(buffer, 0, buffer.Length)) > 0)
            {
	            double percentDone = 100*input.Position/input.Length;//input.Position:input.Length=x:100
				CBB_LoggingCustomTraceLogExtensions.AddLine(log, "..." + Math.Round(percentDone, 0) + "%", false);

                cSocket.Send(buffer, bytes, 0);

            }
            input.Close();

			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Upload done.");

            if (cSocket.Connected)
            {
                cSocket.Close();
            }

            readReply();
            if (!(retValue == 226 || retValue == 250))
            {
                throw new IOException(reply.Substring(4));
            }
        }

        ///
        /// Delete a file from the remote FTP server.
        ///
        ///
        public void deleteRemoteFile(string fileName, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            sendCommand("DELE " + fileName);

            if (retValue != 250)
            {
                throw new IOException(reply.Substring(4));
            }

        }

        ///
        /// Rename a file on the remote FTP server.
        ///
        ///
        ///
        public void renameRemoteFile(string oldFileName, string newFileName, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            sendCommand("RNFR " + oldFileName);

            if (retValue != 350)
            {
                throw new IOException(reply.Substring(4));
            }

            //  known problem
            //  rnto will not take care of existing file.
            //  i.e. It will overwrite if newFileName exist
            sendCommand("RNTO " + newFileName);
            if (retValue != 250)
            {
                throw new IOException(reply.Substring(4));
            }

        }

        ///
        /// Create a directory on the remote FTP server.
        ///
        ///
        public void mkdir(string dirName, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            sendCommand("MKD " + dirName);

            if (retValue != 250)
            {
                throw new IOException(reply.Substring(4));
            }

        }

        ///
        /// Delete a directory on the remote FTP server.
        ///
        ///
        public void rmdir(string dirName, CBB_LoggingCustomTraceLog log)
        {

            if (!logined)
            {
                login(log);
            }

            sendCommand("RMD " + dirName);

            if (retValue != 250)
            {
                throw new IOException(reply.Substring(4));
            }

        }

        ///
        /// Change the current working directory on the remote FTP server.
        ///
        ///
        public void chdir(string dirName, CBB_LoggingCustomTraceLog log)
        {

            if (dirName.Equals("."))
            {
                return;
            }

            if (!logined)
            {
                login(log);
            }

            sendCommand("CWD " + dirName);

            if (retValue != 250)
            {
                throw new IOException(reply.Substring(4));
            }

            this.remotePath = dirName;

			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Current directory is: " + remotePath);

        }

        ///
        /// Close the FTP connection.
        ///
        public void close(CBB_LoggingCustomTraceLog log)
        {

            if (clientSocket != null)
            {
                try
                {
                    sendCommand("QUIT");
                }
                catch (Exception)
                {

                }
            }

            cleanup();
			CBB_LoggingCustomTraceLogExtensions.AddLine(log, "Closed.");
        }

        ///
        /// Set debug mode.
        ///
        ///
        public void setDebug(Boolean debug)
        {
            this.debug = debug;
        }

        private void readReply()
        {
            mes = "";
            reply = readLine();
            retValue = Int32.Parse(reply.Substring(0, 3));
        }

        private void cleanup()
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
            logined = false;
        }

        private string readLine()
        {

            while (true)
            { //after sending password without 1 sec pause next line blocks
                bytes = clientSocket.Receive(buffer, buffer.Length, 0);
                mes += ASCII.GetString(buffer, 0, bytes);
                if (bytes < buffer.Length)
                {
                    break;
                }
            }

            char[] seperator = { '\n' };
            string[] mess = mes.Split(seperator);

            if (mes.Length > 2)
            {
                mes = mess[mess.Length - 2];
            }
            else
            {
                mes = mess[0];
            }

            if (!mes.Substring(3, 1).Equals(" "))
            {
                return readLine();
            }

            if (debug)
            {
                for (int k = 0; k < mess.Length - 1; k++)
                {
                    Console.WriteLine(mess[k]);
                }
            }
            return mes;
        }

        private void sendCommand(String command)
        {
            Byte[] cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
            clientSocket.Send(cmdBytes, cmdBytes.Length, 0);
            System.Threading.Thread.Sleep(1000);//see comment in readReply()
            readReply();
        }

        private Socket createDataSocket()
        {

            sendCommand("PASV");

            if (retValue != 227)
            {
                throw new IOException(reply.Substring(4));
            }

            int index1 = reply.IndexOf('(');
            int index2 = reply.IndexOf(')');
            string ipData = reply.Substring(index1 + 1, index2 - index1 - 1);
            int[] parts = new int[6];

            int len = ipData.Length;
            int partCount = 0;
            string buf = "";

            for (int i = 0; i < len && partCount <= 6; i++)
            {

                char ch = Char.Parse(ipData.Substring(i, 1));
                if (Char.IsDigit(ch)) buf += ch;
                else if (ch != ',')
                {
                    throw new IOException("Malformed PASV reply: " + reply);
                }

                if (ch == ',' || i + 1 == len)
                {

                    try
                    {
                        parts[partCount++] = Int32.Parse(buf);
                        buf = "";
                    }
                    catch (Exception)
                    {
                        throw new IOException("Malformed PASV reply: " + reply);
                    }
                }
            }

            string ipAddress = parts[0] + "." + parts[1] + "." + parts[2] + "." + parts[3];

            int port = (parts[4] << 8) + parts[5];

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ep = new IPEndPoint(Dns.Resolve(ipAddress).AddressList[0], port);

            try
            {
                s.Connect(ep);
            }
            catch (Exception)
            {
                throw new IOException("Can't connect to remote server");
            }

            return s;
        }
    }
}
