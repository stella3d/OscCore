internal class WriteAddressPerfTestData
{
    public struct TestRunSettings {
        public int Segments;
        public int SegmentLength;

        public TestRunSettings(int segments, int segmentLength) {
            Segments = segments;
            SegmentLength = segmentLength;
        }
    }

    public static TestRunSettings[] Settings;

    static string[] GenerateTestAddresses(int addressCount, TestRunSettings settings) {
        return new string[addressCount]
            .Select(_ => RandomOscAddress(settings.Segments, settings.SegmentLength));
    }

    static string RandomOscAddress(int segments, int segmentLength) {
        var addr = "/";
        var lastSlashIndex = segmentLength - 1;
        for (int i = 0; i < segmentLength; i++) {
            var segment = RandomString(segmentLength);
            addr += segment;
            if (i < lastSlashIndex)
                addr += "/";
        }
    }

    static System.Random s_Random = new System.Random();
    static string RandomString(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[s_Random.Next(s.Length)]).ToArray());
    }
}


