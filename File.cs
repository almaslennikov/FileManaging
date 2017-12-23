using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace IO
{
    public class File : IDisposable
    {
        private SafeFileHandle handle;
        private AccessMode accessMode;
        private File(SafeFileHandle handle, AccessMode accessMode)
        {
            this.handle = handle;
            this.accessMode = accessMode;
            if (accessMode == AccessMode.Append)
            {
                WinApiFileFunctions.SetFilePointer(this.handle, 0, out int opt, (uint)FilePointerPosition.End);
            }
            else
            {
                WinApiFileFunctions.SetFilePointer(this.handle, 0, out int opt, (uint)FilePointerPosition.Begin);
            }
            CheckLastError("Opening file");
        }
        public static File Open(
            string fileName,
            AccessMode accessMode,
            CreationDisposition disposition = CreationDisposition.OpenAlways,
            ShareMode shareMode = ShareMode.NoShare,
            FileAttributes attributes = FileAttributes.Normal)
        {
            return new File(
                WinApiFileFunctions.CreateFile(fileName, (uint)accessMode, (uint)shareMode, (IntPtr)null, (uint)disposition, (uint)attributes, (IntPtr)null),
                accessMode);
        }

        public Int32 Write(Byte[] buffer)
        {
            if (accessMode != AccessMode.Read)
            {
                WinApiFileFunctions.WriteFile(handle, buffer, (uint)buffer.Length, out uint numberOfBytesWritten, (IntPtr)null);
                CheckLastError("Writing into file");
                return (Int32)numberOfBytesWritten;
            }
            else
            {
                throw new FileException("File is opened in read mode");
            }
        }
        public Int32 WriteLine(string str)
        {
            Int32 result = Write(Encoding.ASCII.GetBytes(str));
            Byte[] eol = { 13, 10 };
            result += Write(eol);
            return result;
        }
        public bool Read(byte[] buffer, int count)
        {
            if (accessMode == AccessMode.Read || accessMode == AccessMode.All)
            {
                bool result = WinApiFileFunctions.ReadFile(handle, buffer, (uint)count, out uint read, (IntPtr)null);
                CheckLastError("Reading from file");
                return result;
            }
            else
            {
                throw new FileException("File is not opened in read/all mode");
            }
        }
        public static bool Rename(string existingFileName, string newFileName)
        {
            bool result = WinApiFileFunctions.MoveFile(existingFileName, newFileName);
            CheckLastError("Renaming file");
            return result;
        }
        public void Close()
        {
            handle.Close();
        }
        public void Dispose()
        {
            Close();
        }

        private static void CheckLastError(string action)
        {
            int errorCode = Marshal.GetLastWin32Error();
            if (errorCode != 0)
            {
                throw new FileException(String.Format("{0} has been unsuccessful. Win32 Error Code: {1}", action, errorCode));
            }
        }

        internal sealed class WinApiFileFunctions
        {
            [DllImport("kernel32.dll",
                    CharSet = CharSet.Auto,
                    CallingConvention = CallingConvention.StdCall,
                    SetLastError = true)]
            public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFileAttributes,
            IntPtr hTemplateFile);

            [DllImport("kernel32.dll",
                        CharSet = CharSet.Auto,
                        CallingConvention = CallingConvention.StdCall,
                        SetLastError = true)]
            public static extern Boolean WriteFile(
                SafeFileHandle fFile,
                Byte[] lpBuffer,
                UInt32 nNumberOfBytesToWrite,
                out UInt32 lpNumberOfBytesWritten,
                IntPtr lpOverlapped);

            [DllImport("kernel32.dll",
                        CharSet = CharSet.Auto,
                        CallingConvention = CallingConvention.StdCall,
                        SetLastError = true)]
            public static extern uint SetFilePointer(
                SafeFileHandle hFile,
                int lDistanceToMove,
                out int lpDistanceToMoveHigh,
                uint dwMoveMethod);

            [DllImport("kernel32.dll",
                        CharSet = CharSet.Auto,
                        CallingConvention = CallingConvention.StdCall,
                        SetLastError = true)]
            public static extern bool ReadFile(
                SafeFileHandle hFile,
                [Out] byte[] lpBuffer,
                uint nNumberOfBytesToRead,
                out uint lpNumberOfBytesRead,
                IntPtr lpOverlapped);

            [DllImport("kernel32.dll",
                        CharSet = CharSet.Auto,
                        CallingConvention = CallingConvention.StdCall,
                        SetLastError = true)]
            public static extern bool MoveFile(
                string lpExistingFileName,
                string lpNewFileName);
        }

        internal enum FilePointerPosition : uint
        {
            Begin = 0x0,
            Current = 0x1,
            End = 0x2
        }
    }
}
