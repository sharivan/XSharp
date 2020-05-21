/*
 * 
 * API contendo algumas classes que encapsulam funções úteis usadas pelo jogo
 * 
 */ 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mega_Man_Xvan
{
    /// <summary>
    /// Timer de precisão.
    /// Com esta calsse é possível obter um timer que opere em um intervalo de tempo menor do que o suportado por timers padrões oferecidos pela biblioteca padrão da linguagem.
    /// </summary>
    public class AccurateTimer
    {
        private delegate void TimerEventDel(int id, int msg, IntPtr user, int dw1, int dw2);
        private const int TIME_PERIODIC = 1;
        private const int EVENT_TYPE = TIME_PERIODIC;

        [DllImport("winmm.dll")]
        private static extern int timeBeginPeriod(int msec);

        [DllImport("winmm.dll")]
        private static extern int timeEndPeriod(int msec);

        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerEventDel handler, IntPtr user, int eventType);

        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);

        private Form mForm;
        private Action mAction;
        private int mDelay;
        private int mTimerId;
        private TimerEventDel mHandler;  // NOTE: declare at class scope so garbage collector doesn't release it!!!

        private object mutex;
        private bool running;

        public bool IsRunning
        {
            get
            {
                lock (mutex)
                {
                    return running;
                }
            }
        }

        /// <summary>
        /// Cria um novo timer de precisão
        /// </summary>
        /// <param name="form">Formulário</param>
        /// <param name="action">Ação</param>
        /// <param name="delay">Intervalo</param>
        /// <param name="startPaused">Indica se o timer iniciará pausado ou não</param>
        public AccurateTimer(Form form, Action action, int delay, bool startPaused = false)
        {
            mForm = form;
            mAction = action;
            mDelay = delay;

            mutex = new object();

            mHandler = new TimerEventDel(TimerCallback);
            if (!startPaused)
                Start();
        }

        /// <summary>
        /// Inicia o timer
        /// </summary>
        public void Start()
        {
            lock (mutex)
            {
                if (running)
                    return;

                running = true;
                timeBeginPeriod(1); // define a resolução do timer com o tempo de 1 milisegundo
                mTimerId = timeSetEvent(mDelay, 0, mHandler, IntPtr.Zero, EVENT_TYPE); // inicia o timer
            }
        }

        /// <summary>
        /// Para o timer
        /// </summary>
        public void Stop()
        {
            lock (mutex)
            {
                if (!running)
                    return;

                running = false;
                int err = timeKillEvent(mTimerId); // interrompe a execução do timer
                mTimerId = 0;
                timeEndPeriod(1);
            }
        }

        /// <summary>
        /// Callback usado pelo timer para redirecionar o evento para a ação informada na criação deste timer
        /// </summary>
        /// <param name="id">Identificação do evento</param>
        /// <param name="msg">Identificação da mensagem</param>
        /// <param name="user">Usuário associado a mensagem</param>
        /// <param name="dw1">Parâmetro superior da mensagem</param>
        /// <param name="dw2">Parâmetro inferior da mensagem</param>
        private void TimerCallback(int id, int msg, IntPtr user, int dw1, int dw2)
        {
            lock (mutex)
            {
                if (!running)
                    return;
            }

            if (mTimerId != 0)
                try
                {
                    mForm.Invoke(mAction);
                }
                catch (ObjectDisposedException)
                {
                }
        }
    }

    /// <summary>
    /// Encapsula a API do Windows para tocar sons usando chamadas mci
    /// </summary>
    public class MciPlayer : IDisposable
    {
        [DllImport("winmm.dll")]
        private static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        [DllImport("winmm.dll")]
        private static extern Int32 mciGetErrorString(Int32 errCode, StringBuilder errMsg, Int32 buflen);

        [DllImport("winmm.dll")]
        private static extern Int32 mciGetDeviceID(string lpszDevice);

        private string fileName; // Nome do arquivo do audio
        private string alias; // Alias do audio (usado nas chamadas mci)

        private int deviceID; // Identificação do dispositivo
        private bool loaded; // Flag que indica se foi ou não aberto o arquivo de audio e que esteja pronto para tocar
        private bool paused; // Flag que indica se a música foi pausada

        /// <summary>
        /// Cria um novo objeto MciPlayer mas sem carregar o arquivo de audio
        /// </summary>
        public MciPlayer()
        {
            deviceID = 0;
            loaded = false;
            paused = false;
        }

        /// <summary>
        /// Cria um novo objeto MciPlayer carregando o arquivo de audio correspondente ao nome de arquivo informado
        /// </summary>
        /// <param name="fileName">Nome do arquivo de audio</param>
        /// <param name="alias">Alias do audio, usado nas chamadas mci posteriores</param>
        public MciPlayer(string fileName, string alias) : this()
        {
            Load(fileName, alias);
        }

        /// <summary>
        /// Nome do arquivo de audio
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// Alias do audio
        /// </summary>
        public string Alias
        {
            get { return alias; }
        }

        /// <summary>
        /// Identificação do dispositivo
        /// </summary>
        public int DeviceID
        {
            get { return deviceID; }
        }

        /// <summary>
        /// true se o audio foi carregado, false caso contrário
        /// </summary>
        public bool Loaded
        {
            get { return loaded; }
        }

        /// <summary>
        /// Indica se a música está (ou estará) ou não pausada
        /// </summary>
        public bool Paused
        {
            get { return paused; }
            set
            {
                if (value)
                    Pause();
                else
                    Resume();
            }
        }

        /// <summary>
        /// Tamanho do audio que está tocando
        /// </summary>
        public int Length
        {
            get
            {
                if (!loaded)
                    return -1;

                if (IsPlaying())
                {
                    string command = "status " + alias + " length";
                    StringBuilder returnData = new StringBuilder(128);
                    mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);
                    return int.Parse(returnData.ToString());
                }

                return 0;
            }
        }

        /// <summary>
        /// Carrega o audio
        /// </summary>
        /// <param name="fileName">Nome do arquivo do audio</param>
        /// <param name="alias">Alias do audio</param>
        /// <returns>true se o audio foi carregado, false caso contrário</returns>
        public bool Load(string fileName, string alias)
        {
            this.fileName = fileName;
            this.alias = alias;

            Stop();
            Close();

            paused = false;
            string command = "open \"" + fileName + "\" alias " + alias;
            int ret = mciSendString(command, null, 0, IntPtr.Zero);
            loaded = ret == 0;
            if (loaded) // Se o audio foi carregado
                deviceID = mciGetDeviceID(alias); // obtem a identificação do dispositivo relacionado ao audio carregado

            return loaded;

        }

        /// <summary>
        /// Toca o audio previamente carregado
        /// </summary>
        /// <param name="loop">Indica se o audio será tocado em looping</param>
        public void Play(bool loop = false)
        {
            if (loaded)
            {
                paused = false;
                string command = "play " + alias + " from 0" + (loop ? " repeat" : "");
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Toca o audio previamente carregado
        /// </summary>
        /// <param name="callback">Ponteiro para o callback</param>
        /// <param name="loop">Indica se o audio será tocado em looping</param>
        public void Play(IntPtr callback, bool loop = false)
        {
            if (loaded)
            {
                paused = false;
                string command = "play " + alias + " from 0 notify" + (loop ? " repeat" : "");
                mciSendString(command, null, 0, callback);
            }
        }

        /// <summary>
        /// Verifica se o audio está tocando
        /// </summary>
        /// <returns>true se estiver tocando, false caso contrário</returns>
        public bool IsPlaying()
        {
            if (loaded)
            {
                string command = "status " + alias + " mode";
                StringBuilder returnData = new StringBuilder(128);
                mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);
                if (returnData.Length == 7 && returnData.ToString().Substring(0, 7) == "playing")
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Pausa o audio
        /// </summary>
        public void Pause()
        {
            if (loaded && !paused && IsPlaying())
            {
                paused = true;
                string command = "pause " + alias;
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Resume o audio anteriormente pausado
        /// </summary>
        public void Resume()
        {
            if (loaded && paused)
            {
                paused = false;
                string command = "resume " + alias;
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Obtém/Altera o tempo (em milisegundos) da posição atual do audio que está tocando
        /// </summary>
        /// <returns></returns>
        public int CurrentTime
        {
            get
            {
                if (!loaded)
                    return -1;

                string command = "status " + alias + " position";
                StringBuilder returnData = new StringBuilder(128);
                mciSendString(command, returnData, returnData.Capacity, IntPtr.Zero);
                return int.Parse(returnData.ToString());
            }
            set
            {
                if (!loaded)
                    return;

                if (IsPlaying())
                {
                    paused = false;
                    string command = "play " + alias + " from " + value.ToString();
                    mciSendString(command, null, 0, IntPtr.Zero);
                }
                else
                {
                    string command = "seek " + alias + " to " + value.ToString();
                    mciSendString(command, null, 0, IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Altera o volume do audio
        /// </summary>
        /// <param name="volume">Intensidade do volume entre 0 e 100</param>
        /// <returns>true se o audio foi carregado e o volume é válido, false caso contrário</returns>
        public bool SetVolume(int volume)
        {
            if (!loaded)
                return false;

            if (volume >= 0 && volume <= 1000)
            {
                string command = "setaudio " + alias + " volume to " + volume.ToString();
                mciSendString(command, null, 0, IntPtr.Zero);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Altera o balanceamento do audio
        /// </summary>
        /// <param name="balance">Intensidade do balanceamento entre o e 1000</param>
        /// <returns>true se o audio foi carregado e o balanceamento é válido, false caso contrário</returns>
        public bool SetBalance(int balance)
        {
            if (!loaded)
                return false;

            if (balance >= 0 && balance <= 1000)
            {
                string command = "setaudio " + alias + " left volume to " + (1000 - balance).ToString();
                mciSendString(command, null, 0, IntPtr.Zero);
                command = "setaudio " + alias + " right volume to " + balance.ToString();
                mciSendString(command, null, 0, IntPtr.Zero);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Para de tocar o audio
        /// </summary>
        public void Stop()
        {
            if (loaded)
            {
                paused = false;
                string command = "stop " + alias;
                mciSendString(command, null, 0, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Libera todos os recursos assicioados ao audio previamente carregado
        /// </summary>
        public void Close()
        {
            if (!loaded)
                return;

            string command = "close " + alias;
            mciSendString(command, null, 0, IntPtr.Zero);
            loaded = false;
        }

        /// <summary>
        /// O mesmo que Close()
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }

    /// <summary>
    /// Uma coleção de audios
    /// </summary>
    public class SoundCollection : IDisposable
    {
        private Dictionary<string, MciPlayer> sounds; // Tabela hash contendo os audios previamente carregados

        /// <summary>
        /// Cria uma nova coleção de audios
        /// </summary>
        public SoundCollection()
        {
            sounds = new Dictionary<string, MciPlayer>();
        }

        /// <summary>
        /// Adiciona um novo audio a coleção, carregando-o para que ele esteja pronto pra tocar
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio</param>
        public void Add(string soundName)
        {
            Add(soundName, soundName + ".wav");
        }

        /// <summary>
        /// Adiciona um novo audio a coleção, carregando-o para que ele esteja pronto pra tocar
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio</param>
        /// <param name="resourceName">Caminho do recurso associado ao audio</param>
        public void Add(string soundName, string resourceName)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + System.AppDomain.CurrentDomain.FriendlyName + "\\Resources\\Sounds";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string sound_path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + System.AppDomain.CurrentDomain.FriendlyName + "\\Resources\\Sounds\\" + resourceName;

            if (!File.Exists(sound_path))
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Bomberman.Resources.Sounds." + resourceName);
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                File.WriteAllBytes(sound_path, buffer);
            }

            MciPlayer player = new MciPlayer(sound_path, soundName);
            sounds.Add(soundName, player);
        }

        /// <summary>
        /// Verifica se o audio está na coleção
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio</param>
        /// <returns>true se o audio está na coleção, false caso contrário</returns>
        public bool Contains(string soundName)
        {
            return sounds.ContainsKey(soundName);
        }

        /// <summary>
        /// Obtém o objeto MciPlayer correspondente ao nome do audio informado
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio</param>
        /// <returns>O objeto MciPlayer associado ao nome do audio informado</returns>
        public MciPlayer this[string soundName]
        {
            get { return sounds.ContainsKey(soundName) ? sounds[soundName] : null; }
        }

        /// <summary>
        /// Toca um som desta coleção
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio a ser tocado</param>
        public void Play(string soundName, bool loop = false)
        {
            if (sounds.ContainsKey(soundName))
            {
                MciPlayer player = sounds[soundName];
                player.Play(loop);
            }
        }

        /// <summary>
        /// Para de tocar um som desta coleção
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio que está tocando</param>
        public void Stop(string soundName)
        {
            if (sounds.ContainsKey(soundName))
            {
                MciPlayer player = sounds[soundName];
                player.Stop();
            }
        }

        /// <summary>
        /// Remove um audio desta coleção
        /// </summary>
        /// <param name="soundName">Nome (alias) do audio a ser removido</param>
        public void Remove(string soundName)
        {
            if (sounds.ContainsKey(soundName))
            {
                MciPlayer player = sounds[soundName];
                sounds.Remove(soundName);
                player.Close();
            }
        }

        /// <summary>
        /// Fecha a coleção, descarregando todos os audios contidos nela e assim liberando qualquer recurso associado a eles
        /// </summary>
        public void Close()
        {
            Dictionary<string, MciPlayer>.ValueCollection players = sounds.Values;
            foreach (MciPlayer player in players)
                player.Close();

            sounds.Clear();
        }

        /// <summary>
        /// O mesmo que Close()
        /// </summary>
        public void Dispose()
        {
            Close();
        }
    }

    /// <summary>
    /// Representa os dados de uma entrada no rank
    /// </summary>
    public class RankEntry
    {
        private string name; // Nome do jogador
        private int level; // Número do level ao qual ele estava quando morreu
        private int score; // Pontuação atingida por ele antes de morrer

        /// <summary>
        /// Cria uma nova entrada no rank a partir dos dados do jogador
        /// </summary>
        /// <param name="name">Nome do jogador</param>
        /// <param name="level">Número do level ao qual ele estava quando morreu</param>
        /// <param name="score">Pontuação atingida por ele antes de morrer</param>
        public RankEntry(string name, int level, int score)
        {
            this.name = name;
            this.level = level;
            this.score = score;
        }

        /// <summary>
        /// Cria uma nova entrada do rank a partir de outra entrada
        /// </summary>
        /// <param name="entry">Entrada do rank usada como protótipo</param>
        public RankEntry(RankEntry entry)
        {
            UpdateFrom(entry);
        }

        /// <summary>
        /// Cria uma nova entrada do rank a partir de uma stream
        /// </summary>
        /// <param name="stream">Stream usada para ler os dados do rank</param>
        public RankEntry(Stream stream)
        {
            ReadFromStream(stream);
        }

        /// <summary>
        /// Atualiza a entrada do rank a partir de outra entrada
        /// </summary>
        /// <param name="entry">Entrada do rank usada como fonte dos dados</param>
        public void UpdateFrom(RankEntry entry)
        {
            name = entry.name;
            level = entry.level;
            score = entry.score;
        }

        /// <summary>
        /// Nome do jogador
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Level que o jogador atingiu antes de morrer
        /// </summary>
        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        /// <summary>
        /// Pontuação que o jogador atingiu antes de morrer
        /// </summary>
        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        /// <summary>
        /// Obtém os dados da entrada do rank a partir de um fluxo de dados.
        /// Pode ser usado para resgatar o rank a partir de um arquivo ou da rede.
        /// </summary>
        /// <param name="stream"></param>
        public void ReadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            name = reader.ReadString();
            level = reader.ReadInt32();
            score = reader.ReadInt32();
        }

        /// <summary>
        /// Escreve os dados do rank em um fluxo de dados.
        /// Pode ser usado para gravar o rank em um arquivo ou transmiti-lo pela rede.
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(name);
            writer.Write(level);
            writer.Write(score);
        }
    }

    /// <summary>
    /// Define as teclas de cada comando do jogo usadas pelo jogador
    /// </summary>
    public class KeyBinding
    {
        // Teclas padrões para cada comando
        public static readonly Keys DEFAULT_LEFT = Keys.Left;
        public static readonly Keys DEFAULT_UP = Keys.Up;
        public static readonly Keys DEFAULT_RIGHT = Keys.Right;
        public static readonly Keys DEFAULT_DOWN = Keys.Down;
        public static readonly Keys DEFAULT_DROPBOMB = Keys.Space;
        public static readonly Keys DEFAULT_KICK = Keys.K;
        public static readonly Keys DEFAULT_DETONATE = Keys.ControlKey;
        public static readonly Keys DEFAULT_PAUSE = Keys.Return;

        private Keys left; // Movimentação para a esquerda
        private Keys up; // Movimentação para cima
        private Keys right; // Movimentação para a direita
        private Keys down; // Movimentação para baixo
        private Keys dropBomb; // Plantar bomba
        private Keys kick; // Chutar
        private Keys detonate; // Detonar bomba por controle remoto
        private Keys pause; // Pausar o jogo

        /// <summary>
        /// Cria uma nova configuração de teclas usando as teclas padrões
        /// </summary>
        public KeyBinding()
            : this(DEFAULT_LEFT, DEFAULT_UP, DEFAULT_RIGHT, DEFAULT_DOWN, DEFAULT_DROPBOMB, DEFAULT_KICK, DEFAULT_DETONATE, DEFAULT_PAUSE)
        {
        }

        /// <summary>
        /// Cria uma nova configuração de teclas a partir das teclas informadas
        /// </summary>
        /// <param name="left">Tecla de movimentação para a esquerda</param>
        /// <param name="up">Tecla de movimentação para cima</param>
        /// <param name="right">Tecla de movimentação para a direita</param>
        /// <param name="down">Tecla de movimentação para baixo</param>
        /// <param name="dropBomb">Tecla de plantar bomba</param>
        /// <param name="kick">Tecla de chutar</param>
        /// <param name="detonate">Tecla de detonar bomba por controle remoto</param>
        /// <param name="pause">Tecla de pausar o jogo</param>
        public KeyBinding(Keys left, Keys up, Keys right, Keys down, Keys dropBomb, Keys kick, Keys detonate, Keys pause)
        {
            this.left = left;
            this.up = up;
            this.right = right;
            this.down = down;
            this.dropBomb = dropBomb;
            this.kick = kick;
            this.detonate = detonate;
            this.pause = pause;
        }

        /// <summary>
        /// Cria uma nova configuração de teclas a partir de outra já existente
        /// </summary>
        /// <param name="keyBinding">Configuração de teclas usada como protótipo</param>
        public KeyBinding(KeyBinding keyBinding)
        {
            UpdateFrom(keyBinding);
        }

        /// <summary>
        /// Cria uma nova configuração de teclas a partir de um fluxo de dados
        /// </summary>
        /// <param name="stream">Fluxo de dados a ser lido</param>
        public KeyBinding(Stream stream)
        {
            ReadFromStream(stream);
        }

        /// <summary>
        /// Atualiza a configuração de teclas a partir de outra já existente
        /// </summary>
        /// <param name="keyBinding">Configuração de teclas usada como protótipo</param>
        public void UpdateFrom(KeyBinding keyBinding)
        {
            left = keyBinding.left;
            up = keyBinding.up;
            right = keyBinding.right;
            down = keyBinding.down;
            dropBomb = keyBinding.dropBomb;
            kick = keyBinding.kick;
            detonate = keyBinding.detonate;
            pause = keyBinding.pause;
        }

        /// <summary>
        /// Tecla de movimentação para esquerda
        /// </summary>
        public Keys Left
        {
            get { return left; }
            set { left = value; }
        }

        /// <summary>
        /// Tecla de movimentação para cima
        /// </summary>
        public Keys Up
        {
            get { return up; }
            set { up = value; }
        }

        /// <summary>
        /// Tecla de movimentação para direita
        /// </summary>
        public Keys Right
        {
            get { return right; }
            set { right = value; }
        }

        /// <summary>
        /// Tecla de movimentação para baixo
        /// </summary>
        public Keys Down
        {
            get { return down; }
            set { down = value; }
        }

        /// <summary>
        /// Tecla de plantar bomba
        /// </summary>
        public Keys DropBomb
        {
            get { return dropBomb; }
            set { dropBomb = value; }
        }

        /// <summary>
        /// Tecla de chutar
        /// </summary>
        public Keys Kick
        {
            get { return kick; }
            set { kick = value; }
        }

        /// <summary>
        /// Tecla de detonar bomba por controle remoto
        /// </summary>
        public Keys Detonate
        {
            get { return detonate; }
            set { detonate = value; }
        }

        /// <summary>
        /// Tecla de pausar o jogo
        /// </summary>
        public Keys Pause
        {
            get { return pause; }
            set { pause = value; }
        }

        /// <summary>
        /// Obtém a configuração de teclas a partir de um fluxo de dados.
        /// Útil para restaurar a configuração através de um arquivo ou da rede.
        /// </summary>
        /// <param name="stream"></param>
        public void ReadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            left = (Keys)reader.ReadByte();
            up = (Keys)reader.ReadByte();
            right = (Keys)reader.ReadByte();
            down = (Keys)reader.ReadByte();
            dropBomb = (Keys)reader.ReadByte();
            kick = (Keys)reader.ReadByte();
            detonate = (Keys)reader.ReadByte();
            pause = (Keys)reader.ReadByte();
        }

        /// <summary>
        /// Escreve a configuração de teclas em um fluxo de dados.
        /// Útil para armazenar a configuração de teclas em um arquivo ou transmiti-la pela rede.
        /// </summary>
        /// <param name="stream"></param>
        public void WriteToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write((byte)left);
            writer.Write((byte)up);
            writer.Write((byte)right);
            writer.Write((byte)down);
            writer.Write((byte)dropBomb);
            writer.Write((byte)kick);
            writer.Write((byte)detonate);
            writer.Write((byte)pause);
        }
    }
}
