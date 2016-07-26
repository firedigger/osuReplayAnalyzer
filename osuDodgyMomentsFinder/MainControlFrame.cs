using Kajabity.Tools.Java;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osuDodgyMomentsFinder
{
    public class MainControlFrame
    {
        private static MainControlFrame instance;

        public OsuDbAPI.OsuDbFile osuDbP { get; set; }
        public string pathSongs { get; set; }
        public string pathReplays { get; set; }
        public string pathOsuDB { get; set; }
        public string pathSettings = "settings.txt";

        private MainControlFrame()
        {
        }

        public static MainControlFrame Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainControlFrame();
                }
                return instance;
            }
        }

        public void LoadSettings()
        {
            FileStream stream = new FileStream(pathSettings,FileMode.Open);
            JavaProperties settings = new JavaProperties();
            settings.Load(stream);
            pathSongs = settings.GetProperty("pathSongs");
            pathReplays = settings.GetProperty("pathReplays");
            pathOsuDB = settings.GetProperty("pathOsuDB");
            Console.WriteLine(pathOsuDB);
            osuDbP = new OsuDbAPI.OsuDbFile(pathOsuDB);
        }
    }
}
