namespace BlazorAuth;

public class BlazorAuthOptions
{
    public enum eStorageType
    {
        Session,
        Local
    }

    public eStorageType StorageType { get; set; } = eStorageType.Session;
}
