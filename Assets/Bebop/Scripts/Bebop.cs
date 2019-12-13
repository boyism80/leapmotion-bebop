using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using OpenCvSharp;



using BebopCommandSet;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ParrotBebop2
{

    public class Bebop : MonoBehaviour, D2CSocket.OnReceiveListener
    {
        public enum BebopState
        {
            Landed = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_LANDED,
            TakingOff = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_TAKINGOFF,
            Hovering = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_HOVERING,
            Flying = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_FLYING,
            Landing = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_LANDING,
            Emergency = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_EMERGENCY,
            Max = CommandSet.ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_MAX,
        }

        private int[] seq = new int[256];
        public PCMD pcmd;

        private Mutex pcmdMtx = new Mutex();

        private UdpClient arstreamClient;
        private IPEndPoint remoteIpEndPoint;

        //private UdpClient c2d_client;

        private byte[] receivedData;
        private static object _thisLock = new object();

        private D2CSocket _d2c;
        private C2DSocket _c2d;
        private Thread _pcmd_thread;
        private byte _battery;
        private short _rssi;
        private uint _state;
        public BebopState State
        {
            get
            {
                return (BebopState)this._state;
            }
            private set
            {
                this._state = (uint)value;
            }
        }
        private double _lat, _lon, _alt;
        private Vector3 _speed;
        private float _pitch, _yaw, _roll;
        private double _altitude;
        private byte _tilt;
        private byte _pan;

        public Text _state_text;

        public void Start()
        {
            this.Discover();
        }

        public void Update()
        {
            this._state_text.text = string.Format(" - Battery : {0}%\r\n - State : {1}", this._battery, this.State);
        }

        public void OnDestroy()
        {
            this._d2c.Running = false;
        }


        public int Discover()
        {
            Debug.Log("Discovering...");

            //c2d_client = new UdpClient(CommandSet.IP, 54321);

            this._d2c = new D2CSocket(this);
            this._d2c.Running = true;

            this._c2d = new C2DSocket();


            //make handshake with TCP_client, and the port is set to be 4444
            TcpClient tcpClient = new TcpClient(CommandSet.IP, CommandSet.DISCOVERY_PORT);
            NetworkStream stream = new NetworkStream(tcpClient.Client);

            //initialize reader and writer
            StreamWriter streamWriter = new StreamWriter(stream);
            StreamReader streamReader = new StreamReader(stream);

            //when the drone receive the message bellow, it will return the confirmation
            string handshake_Message = "{\"controller_type\":\"computer\", \"controller_name\":\"katarina\", \"d2c_port\":\"43210\", \"arstream2_client_stream_port\":\"55004\", \"arstream2_client_control_port\":\"55005\"}";
            streamWriter.WriteLine(handshake_Message);
            streamWriter.Flush();




            string receive_Message = streamReader.ReadLine();
            if (receive_Message == null)
            {
                Debug.Log("Discover failed");
                return -1;
            }
            else
            {
                Debug.Log("The message from the drone shows: " + receive_Message);

                //initialize
                pcmd = default(PCMD);

                this.generateAllStates();
                this.generateAllSettings();

                //enable video streaming
                videoEnable();

                //init ARStream
                //initARStream();

                //pcmdThreadActive();
                //this.arStreamThreadActive();
                this._pcmd_thread = new Thread(this.pcmdThreadActive);
                this._pcmd_thread.Start();
                return 1;
            }
        }

        public void takeoff()
        {
            Debug.Log("try to takeoff ing...");
            var cmd = new Command(4);

            cmd.append((byte)CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
            cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
            cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_TAKEOFF);
            cmd.append((byte)0);

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID, cmd);
            this.State = BebopState.Hovering;

            //sendCommandAdpator(ref cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void landing()
        {
            Debug.Log("try to landing...");
            var cmd = new Command(4);

            cmd.append((byte)CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
            cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
            cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_LANDING);
            cmd.append((byte)0);

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID, cmd);
            this.State = BebopState.Landed;

            //sendCommandAdpator(ref cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
        }

        public void switch_state()
        {
            Debug.Log(string.Format("현재상태 : {0}", this.State));
            if(this.State == BebopState.Landed)
            {
                this.takeoff();
                Debug.Log("take off...");
            }
            else
            {
                this.landing();
                Debug.Log("landing...");
            }
        }

        public void generatePCMD(/*int flag*/)
        {
            lock (_thisLock)
            {
                //var fixed_pitch = flag == 1 ? this.pcmd.pitch : 0;
                //var fixed_yaw = flag == 0 ? this.pcmd.yaw : 0;
                //var fixed_roll = flag == 1 ? this.pcmd.roll : 0;
                //var fixed_gaz = flag == 0 ? this.pcmd.gaz : 0;

                var cmd = new Command(13);

                cmd.append((byte)CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3);
                cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTING);
                cmd.append((byte)CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTING_CMD_PCMD);
                cmd.append((byte)0);

                //pcmdMtx.WaitOne();
                //cmd.append((byte)flag);  // flag
                //cmd.append((byte)(fixed_roll >= 0 ? fixed_roll : 256 + fixed_roll));  // roll: fly left or right [-100 ~ 100]
                //cmd.append((byte)(fixed_pitch >= 0 ? fixed_pitch : 256 + fixed_pitch));  // pitch: backward or forward [-100 ~ 100]
                //cmd.append((byte)(fixed_yaw >= 0 ? fixed_yaw : 256 + fixed_yaw));  // yaw: rotate left or right [-100 ~ 100]
                //cmd.append((byte)(fixed_gaz >= 0 ? fixed_gaz : 256 + fixed_gaz));  // gaze: down or up [-100 ~ 100]

                cmd.append((byte)this.pcmd.flag);  // flag
                cmd.append((byte)(this.pcmd.roll >= 0 ? this.pcmd.roll : 256 + this.pcmd.roll));  // roll: fly left or right [-100 ~ 100]
                cmd.append((byte)(this.pcmd.pitch >= 0 ? this.pcmd.pitch : 256 + this.pcmd.pitch));  // pitch: backward or forward [-100 ~ 100]
                cmd.append((byte)(this.pcmd.yaw >= 0 ? this.pcmd.yaw : 256 + this.pcmd.yaw));  // yaw: rotate left or right [-100 ~ 100]
                cmd.append((byte)(this.pcmd.gaz >= 0 ? this.pcmd.gaz : 256 + this.pcmd.gaz));  // gaze: down or up [-100 ~ 100]


                // for Debug Mode
                cmd.append((byte)0);
                cmd.append((byte)0);
                cmd.append((byte)0);
                cmd.append((byte)0);


                this._c2d.send(cmd);
                //sendCommandAdpator(ref cmd);
            }
        }

        public void pcmdThreadActive()
        {
            Console.WriteLine("The PCMD thread is starting");

            while(this._d2c.Running)
            {
                lock(this)
                {
                    this.generatePCMD();
                }
                Thread.Sleep(50);  //sleep 50ms each time.
            }
        }

        public void generateAllStates()
        {
            Debug.Log("Generate All State");
            var cmd = new Command(4);

            cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMON;
            cmd.cmd[2] = (CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff);
            cmd.cmd[3] = (CommandSet.ARCOMMANDS_ID_COMMON_COMMON_CMD_ALLSTATES & 0xff00 >> 8);

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID, cmd);
        }

        public void generateAllSettings()
        {
            Debug.Log("Generate All Settings");
            var cmd = new Command(4);

            cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_COMMON;
            cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_COMMON_CLASS_SETTINGS;
            cmd.cmd[2] = (0 & 0xff); // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_ALLSETTINGS = 0
            cmd.cmd[3] = (0 & 0xff00 >> 8);

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID, cmd);
        }

        public void videoEnable()
        {
            Debug.Log("Send Video Enable Command");
            var cmd = new Command(5);

            cmd.cmd[0] = CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3;
            cmd.cmd[1] = CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_MEDIASTREAMING;
            cmd.cmd[2] = (0 & 0xff); // ARCOMMANDS_ID_COMMON_CLASS_SETTINGS_CMD_VIDEOENABLE = 0
            cmd.cmd[3] = (0 & 0xff00 >> 8);
            cmd.cmd[4] = 1; //arg: Enable

            //sendCommandAdpator(ref cmd, CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID);
            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK, CommandSet.BD_NET_CD_ACK_ID, cmd);
        }

        public void initARStream()
        {
            arstreamClient = new UdpClient(55004);
            remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        }

        public void OnReceiveCommand(int type, int id, int seq, Command cmd)
        {
            switch (type)
            {
                case CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PING:
                    this.OnInternalBufferIdPing(type, id, seq, cmd);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_ACK:
                    this.OnFrameTypeAck(type, id, seq, cmd);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_LOW_LATENCY:
                    this.OnFrameTypeDataLowLatency(type, id, seq, cmd);
                    break;

                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA:
                case CommandSet.ARNETWORKAL_FRAME_TYPE_DATA_WITH_ACK:
                    this.OnReceiveFrameTypeData(type, id, seq, cmd);
                    break;
            }
        }

        private void OnFrameTypeAck(int type, int id, int seq, Command cmd)
        {
            if (type != 0x4)
                return;

            var newcmd = new Command(1);
            newcmd.append((byte)seq);

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_ACK, 0xFE, cmd);
        }

        private void OnInternalBufferIdPing(int type, int id, int seq, Command cmd)
        {
            if (type != 0x2)
                return;

            if (id != CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PING)
                return;

            this._c2d.send(CommandSet.ARNETWORKAL_FRAME_TYPE_DATA, CommandSet.ARNETWORK_MANAGER_INTERNAL_BUFFER_ID_PONG, cmd);
        }

        private void OnFrameTypeDataLowLatency(int type, int id, int seq, Command cmd)
        {
            if (id != CommandSet.BD_NET_DC_VIDEO_DATA_ID)
                return;
        }

        private void OnReceiveFrameTypeData(int type, int id, int seq, Command cmd)
        {
            switch (id)
            {
                case CommandSet.BD_NET_DC_EVENT_ID:
                    this.OnNetDCEventID(type, id, seq, cmd);
                    break;

                case CommandSet.BD_NET_DC_NAVDATA_ID:
                    this.OnNetDCNavDataID(type, id, seq, cmd);
                    break;

                case 0x00:
                    break;
            }
        }

        private void OnNetDCNavDataID(int type, int id, int seq, Command cmd)
        {
            using (var reader = new BinaryReader(new MemoryStream(cmd.cmd)))
            {
                var project = reader.ReadByte();
                var cls = reader.ReadByte();
                var cmd_id = reader.ReadUInt16();

                if (project == CommandSet.ARCOMMANDS_ID_PROJECT_COMMON)
                {
                    switch (cls)
                    {
                        case CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_ANIMATIONS:
                            this.OnReceiveAnimations(cmd_id, reader);
                            break;
                    }
                }
                else if (project == CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3)
                {
                    switch (cls)
                    {
                        case CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTINGSTATE:
                            this.OnReceivePilotingState(cmd_id, reader);
                            break;

                        case CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_CAMERASTATE:
                            this.OnReceiveCameraState(cmd_id, reader);
                            break;
                    }
                }
                else
                { }
            }
        }

        private void OnNetDCEventID(int type, int id, int seq, Command cmd)
        {
            using (var reader = new BinaryReader(new MemoryStream(cmd.cmd)))
            {
                var project = reader.ReadByte();
                var cls = reader.ReadByte();
                var cmd_id = reader.ReadUInt16();

                if (project == CommandSet.ARCOMMANDS_ID_PROJECT_COMMON)
                {
                    switch (cls)
                    {
                        case CommandSet.ARCOMMANDS_ID_COMMON_CLASS_COMMONSTATE:
                            this.OnReceiveCommonState(cmd_id, reader);
                            break;
                    }
                }
                else if (project == CommandSet.ARCOMMANDS_ID_PROJECT_ARDRONE3)
                {
                    switch (cls)
                    {
                        case CommandSet.ARCOMMANDS_ID_ARDRONE3_CLASS_PILOTINGSTATE:
                            this.OnReceivePilotingState(cmd_id, reader);
                            break;
                    }
                }
                else
                { }
            }
        }

        private void OnReceiveCommonState(int id, BinaryReader reader)
        {
            switch (id)
            {
                case CommandSet.ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_BATTERYSTATECHANGED:
                    this._battery = reader.ReadByte();
                    break;

                case CommandSet.ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_MASSSTORAGESTATELISTCHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_MASSSTORAGEINFOSTATELISTCHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_CURRENTDATECHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_COMMON_COMMONSTATE_CMD_CURRENTTIMECHANGED:
                    break;
            }
        }

        private void OnReceiveAnimations(int id, BinaryReader reader)
        {
            switch (id)
            {
                case 7:
                    this._rssi = reader.ReadInt16();
                    break;
            }
        }

        private void OnReceivePilotingState(int id, BinaryReader reader)
        {
            switch (id)
            {
                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_FLATTRIMCHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_FLYINGSTATECHANGED:
                    
                    this._state = reader.ReadUInt32();          // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_LANDED
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_TAKINGOFF
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_HOVERING
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_FLYING
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_LANDING
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_EMERGENCY
                                                                // ARCOMMANDS_ARDRONE3_PILOTINGSTATE_FLYINGSTATECHANGED_STATE_MAX
                                                                //var state_text = new string[] { "landed", "takingoff", "hovering", "flying", "landing", "emergency" };
                                                                //Debug.Log("state : " + state_text[this._state]);
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_ALERTSTATECHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_NAVIGATEHOMESTATECHANGED:
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_POSITIONCHANGED:   // position
                    this._lat = reader.ReadDouble();
                    this._lon = reader.ReadDouble();
                    this._alt = reader.ReadDouble();
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_SPEEDCHANGED:      // speed
                    this._speed.x = reader.ReadSingle();
                    this._speed.y = reader.ReadSingle();
                    this._speed.z = reader.ReadSingle();
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_ATTITUDECHANGED:   // angle
                    this._roll = reader.ReadSingle();
                    this._pitch = reader.ReadSingle();
                    this._yaw = reader.ReadSingle();
                    break;

                case CommandSet.ARCOMMANDS_ID_ARDRONE3_PILOTINGSTATE_CMD_ALTITUDECHANGED:   // altitude
                    this._altitude = reader.ReadDouble();
                    break;
            }
        }

        private void OnReceiveCameraState(int id, BinaryReader reader)
        {
            switch (id)
            {
                case 0:
                    this._tilt = reader.ReadByte();
                    this._pan = reader.ReadByte();
                    //Debug.Log(string.Format("camera state tilt / pan : {0}, {1}", tilt, pan));
                    break;
            }
        }
    }
}