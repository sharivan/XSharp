using XSharp.Exporter;

var writer = new LevelWriter();
writer.Load(@"C:\ROMS & ISOS\SNES-ROMS\Mega Man X (U) (V1.0) [!].smc", 0);
writer.Save("X1_Intro.zip");