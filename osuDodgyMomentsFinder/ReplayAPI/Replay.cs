using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ReplayAPI
{
    public class Replay : IDisposable
    {
        // for customizing which replays to flip
        public bool AxisFlip { get; set; }

        public GameModes GameMode;
        public string Filename;
        public int FileFormat;
        public string MapHash;
        public string PlayerName;
        public string ReplayHash;
        public uint TotalScore;
        public UInt16 Count300;
        public UInt16 Count100;
        public UInt16 Count50;
        public UInt16 CountGeki;
        public UInt16 CountKatu;
        public UInt16 CountMiss;
        public UInt16 MaxCombo;
        public bool IsPerfect;
        public Mods Mods;
        public List<LifeFrame> LifeFrames = new List<LifeFrame>();
        public DateTime PlayTime;
        public int ReplayLength;
        public List<ReplayFrame> ReplayFrames = new List<ReplayFrame>();
        public int Seed;

        private BinaryReader replayReader;
        private CultureInfo culture = new CultureInfo("en-US", false);
        private bool headerLoaded;
        private bool fullLoaded;

        public Replay()
        { }

        public Replay(string replayFile, bool fullLoad = false)
        {
            Filename = replayFile;
            using (replayReader = new BinaryReader(new FileStream(replayFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                loadHeader();
                if (fullLoad)
                {
                    Load();
                }
            }
        }

        private void loadHeader()
        {
            GameMode = (GameModes)Enum.Parse(typeof(GameModes), replayReader.ReadByte().ToString(culture));
            FileFormat = replayReader.ReadInt32();
            MapHash = replayReader.ReadNullableString();
            PlayerName = replayReader.ReadNullableString();
            ReplayHash = replayReader.ReadNullableString();
            Count300 = replayReader.ReadUInt16();
            Count100 = replayReader.ReadUInt16();
            Count50 = replayReader.ReadUInt16();
            CountGeki = replayReader.ReadUInt16();
            CountKatu = replayReader.ReadUInt16();
            CountMiss = replayReader.ReadUInt16();
            TotalScore = replayReader.ReadUInt32();
            MaxCombo = replayReader.ReadUInt16();
            IsPerfect = replayReader.ReadBoolean();
            Mods = (Mods)replayReader.ReadInt32();
            headerLoaded = true;
        }

        /// <summary>
        /// Loads Metadata if not already loaded and loads Lifedata, Timestamp, Playtime and Clicks.
        /// </summary>
        public void Load()
        {
            if (!headerLoaded)
                loadHeader();
            if (fullLoaded)
                return;

            //Life
            string lifeData = replayReader.ReadNullableString();
            if (!string.IsNullOrEmpty(lifeData))
            {
                foreach (string lifeBlock in lifeData.Split(','))
                {
                    string[] split = lifeBlock.Split('|');
                    if (split.Length < 2)
                        continue;

                    LifeFrames.Add(new LifeFrame()
                    {
                        Time = int.Parse(split[0], culture),
                        Percentage = float.Parse(split[1], culture)
                    });
                }
            }

            Int64 ticks = replayReader.ReadInt64();
            PlayTime = new DateTime(ticks, DateTimeKind.Utc);

            ReplayLength = replayReader.ReadInt32();

            //Data
            if (ReplayLength > 0)
            {
                int lastTime = 0;
                using (MemoryStream codedStream = LZMACoder.Decompress(replayReader.BaseStream as FileStream))
                using (StreamReader sr = new StreamReader(codedStream))
                {
                    foreach (string frame in sr.ReadToEnd().Split(','))
                    {
                        if (string.IsNullOrEmpty(frame))
                            continue;

                        string[] split = frame.Split('|');
                        if (split.Length < 4)
                            continue;

                        if (split[0] == "-12345")
                        {
                            Seed = int.Parse(split[3], culture);
                            continue;
                        }

                        ReplayFrames.Add(new ReplayFrame()
                        {
                            TimeDiff = int.Parse(split[0], culture),
                            Time = int.Parse(split[0], culture) + lastTime,
                            X = float.Parse(split[1], culture),
                            Y = float.Parse(split[2], culture),
                            Keys = (Keys)Enum.Parse(typeof(Keys), split[3])
                        });
                        lastTime = ReplayFrames[ReplayFrames.Count - 1].Time;
                    }
                }
            }

            //Todo: There are some extra bytes here

            fullLoaded = true;
        }

        public void Save(string file)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                //Header
                bw.Write((byte)GameMode);
                bw.Write(FileFormat);
                bw.WriteNullableString(MapHash);
                bw.WriteNullableString(PlayerName);
                bw.WriteNullableString(ReplayHash);
                bw.Write(Count300);
                bw.Write(Count100);
                bw.Write(Count50);
                bw.Write(CountGeki);
                bw.Write(CountKatu);
                bw.Write(CountMiss);
                bw.Write(TotalScore);
                bw.Write((UInt16)MaxCombo);
                bw.Write(IsPerfect);
                bw.Write((int)Mods);

                //Life
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < LifeFrames.Count; i++)
                    sb.AppendFormat("{0}|{1},", LifeFrames[i].Time.ToString(culture), LifeFrames[i].Percentage.ToString(culture));
                bw.WriteNullableString(sb.ToString());

                bw.Write(PlayTime.ToUniversalTime().Ticks);

                //Data
                if (ReplayFrames.Count == 0)
                    bw.Write(0);
                else
                {
                    sb.Clear();
                    for (int i = 0; i < ReplayFrames.Count; i++)
                        sb.AppendFormat("{0}|{1}|{2}|{3},", ReplayFrames[i].TimeDiff.ToString(culture), ReplayFrames[i].X.ToString(culture), ReplayFrames[i].Y.ToString(culture), (int)ReplayFrames[i].Keys);
                    sb.AppendFormat("{0}|{1}|{2}|{3},", -12345, 0, 0, Seed);
                    byte[] rawBytes = Encoding.ASCII.GetBytes(sb.ToString());
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(rawBytes, 0, rawBytes.Length);

                        MemoryStream codedStream = LZMACoder.Compress(ms);

                        byte[] rawBytesCompressed = new byte[codedStream.Length];
                        codedStream.Read(rawBytesCompressed, 0, rawBytesCompressed.Length);
                        bw.Write(rawBytesCompressed.Length - 8);
                        bw.Write(rawBytesCompressed);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool state)
        {
            if (replayReader != null)
                replayReader.Close();
            ReplayFrames.Clear();
            LifeFrames.Clear();
        }
    }
}