namespace IO
{
    class FileException : System.Exception
    {
        public FileException() {}
        public FileException(string name)
        : base(name) {}
    }
}
