using Raylib_CsLo;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace chess
{
    public class Profile
    {
        public string name { get; set; }
        public int WinCount { get; set; }
        public int LossCount { get; set; }

        public Profile(string name, int winCount, int lossCount)
        {
            this.name = name;
            this.WinCount = winCount;
            LossCount = lossCount;
        }

        public bool Save()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appdata == null) return false;

            var projectFolder = Path.Combine(appdata, "chess");
            Directory.CreateDirectory(projectFolder);

            var profilePath = Path.Combine(projectFolder, "profile.json");
            var profileJson = JsonSerializer.Serialize(this);

            try
            {
                File.WriteAllText(profilePath, profileJson);
            } catch
            {
                return false;
            }

            return true;
        }

        public bool Load()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (appdata == null) return false;

            var profilePath = Path.Combine(appdata, "chess", "profile.json");

            string profileJson;
            try
            {
                profileJson = File.ReadAllText(profilePath);
            } catch
            {
                return false;
            }

            var loadedProfile = JsonSerializer.Deserialize<Profile>(profileJson);
            if (loadedProfile == null) return false;

            name = loadedProfile.name;
            WinCount = loadedProfile.WinCount;
            LossCount = loadedProfile.LossCount;

            return true;
        }

        public byte[] Encode()
        {
            var nameBytes = Encoding.ASCII.GetBytes(name);
            byte[] payload = new byte[4 + 4 + 4 + nameBytes.Length];
            BitConverter.GetBytes(WinCount).CopyTo(payload, 0);
            BitConverter.GetBytes(LossCount).CopyTo(payload, 4);
            BitConverter.GetBytes(nameBytes.Length).CopyTo(payload, 8);
            nameBytes.CopyTo(payload, 12);
            return payload;
        }

        public static Profile Decode(byte[] payload)
        {
            var winCount = BitConverter.ToInt32(payload, 0);
            var lossCount = BitConverter.ToInt32(payload, 4);
            var nameLength = BitConverter.ToInt32(payload, 8);
            var name = Encoding.ASCII.GetString(payload, 12, nameLength);
            Debug.Assert(name != null);

            return new Profile(name, winCount, lossCount);
        }
    }
}
