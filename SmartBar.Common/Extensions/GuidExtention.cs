using System.Security.Cryptography;

namespace SmartBar.Common.Extentions
{
    public static class GuidExtensions
    {
        public static Guid NewGuid7(this Guid _)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            byte[] uuid = new byte[16];

            byte[] timeBytes = BitConverter.GetBytes(timestamp);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timeBytes);
            }
            Array.Copy(timeBytes, 2, uuid, 0, 6); 

            uuid[6] = (byte)((uuid[6] & 0x0F) | 0x70);

            RandomNumberGenerator.Fill(new Span<byte>(uuid).Slice(8));

            return new Guid(uuid);
        }
    }
}
