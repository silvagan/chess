using Raylib_CsLo;
using System.Numerics;
using System.Text.Json;

namespace chess
{
    internal class Profile
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
    }
}
