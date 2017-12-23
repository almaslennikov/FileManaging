namespace IO
{
    public enum FileAttributes: uint
    {
        Readonly = 0x1,
        Hidden = 0x2,
        System = 0x4,
        Archive = 0x20,
        Normal = 0x80,
        Temporary = 0x100,
        Offline = 0x1000,
        Encrypted = 0x4000
    }
}
