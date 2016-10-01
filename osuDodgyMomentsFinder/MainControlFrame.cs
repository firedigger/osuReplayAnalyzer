using Kajabity.Tools.Java;
using System;
using System.IO;

namespace osuDodgyMomentsFinder
{
    public class MainControlFrame
    {

        public OsuDbAPI.OsuDbFile osuDbP
        {
            get; set;
        }
        public string pathSongs
        {
            get; set;
        }
        public string pathReplays
        {
            get; set;
        }
        public string pathOsuDB
        {
            get; set;
        }
        public string pathSettings = "settings.txt";

        public MainControlFrame()
        {
            pathSongs = "";
            pathReplays = "";
            pathOsuDB = "";
        }

        public void LoadSettings()
        {
            FileStream stream = new FileStream(pathSettings, FileMode.Open);
            try
            {
                JavaProperties settings = new JavaProperties();
                settings.Load(stream);
                pathSongs = settings.GetProperty("pathSongs");
                pathReplays = settings.GetProperty("pathReplays");
                pathOsuDB = settings.GetProperty("pathOsuDB");
                Console.WriteLine(pathOsuDB);
                osuDbP = new OsuDbAPI.OsuDbFile(pathOsuDB);
            }
            catch (Exception exp)
            {
                Console.WriteLine("Error parsing settings:\n" + exp.ToString());
            }
            finally
            {
                stream.Close();
            }
        }

        public void saveSettings()
        {
            FileStream stream = new FileStream(pathSettings, FileMode.Create);
            JavaProperties settings = new JavaProperties();
            settings.SetProperty("pathSongs", pathSongs);
            settings.SetProperty("pathReplays", pathReplays);
            settings.SetProperty("pathOsuDB", pathOsuDB);
            JavaPropertyWriter writer = new JavaPropertyWriter(settings);
            writer.Write(stream, "osuReplayAnalyzer settings");
            stream.Close();
        }
    }
}
