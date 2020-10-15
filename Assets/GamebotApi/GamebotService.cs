using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using System.IO;
// using TensorFlowLite;

namespace bytedance.lab.gamebot
{
    // 定义智能体相关的变量
    public static class GamebotData
    {
        // 控制一下几帧交互一次
        public static int frame = 6;
        public static int now_frame = 0;

        public static int agent_num = 2;
        public static string service_type = "SOCKET_VS_BT";
        public static bool train = true;

        // 请求时发送的消息
        public static Request game_request;
        public static RequestQuit game_quit;

        // 在游戏进行时需要填充的字段
        public static RequestRestartGame reset_request;
        public static RequestStep step_request;

        // 从两个socket中拿到response各一份
        public static Response game_response1;
        public static Response game_response2;
    }

    // Gamebot API
    public static class GamebotService
    {
        public const int buf_size = 10240;
        public static int agent_num = 2;
        public static bool isSync = true;
        public static byte[] bytes;

        // 两个socket端口
        public static Socket client1;
        public static Socket client2;

        static GamebotService()
        {
            // 读取命令行参数
            String[] args = Environment.GetCommandLineArgs();
            int port_num1 = 9000;
            int port_num2 = 9001;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("--port_num1"))
                {
                    String[] port_args = args[i].Split(' ');
                    port_num1 = Int32.Parse(port_args[1]);
                }
                if (args[i].Contains("--port_num2"))
                {
                    String[] port_args2 = args[i].Split(' ');
                    port_num2 = Int32.Parse(port_args2[1]);
                }
                if (args[i].Contains("--sevice_type"))
                {
                    String[] service_type_args = args[i].Split(' ');
                    GamebotData.service_type = service_type_args[1];
                }
                if (args[i].Contains("--frame"))
                {
                    String[] frame_args = args[i].Split(' ');
                    GamebotData.frame = Int32.Parse(frame_args[1]);
                }
            }

            if (GamebotData.service_type == "SOCKET_VS_BT" || GamebotData.service_type == "SOCKET_VS_MULTIAGENT") {
                if(GamebotData.train)
                {
                    // init socket
                    IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                    IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port_num1);

                    // 游戏侧为客户端时
                    if(client1 == null)
                    {
                        client1 = new Socket(ipAddress.AddressFamily,
                            SocketType.Stream, ProtocolType.Tcp);
                    }
                    client1.Connect(localEndPoint);

                    // 设置收发缓存区，与模型训练侧要保持一致
                    client1.ReceiveBufferSize = buf_size;
                    client1.SendBufferSize = buf_size;
                    // Debug.Log("GamebotSocket init");
                }

            } else if (GamebotData.service_type == "SOCKET_VS_SOCKET") {
                // init socket
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

                // 训练侧
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port_num1);
                if(client1 == null)
                {
                    client1 = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                }
                client1.Connect(localEndPoint);

                // 设置收发缓存区，与模型训练侧要保持一致
                client1.ReceiveBufferSize = buf_size;
                client1.SendBufferSize = buf_size;

                // 加载侧
                localEndPoint = new IPEndPoint(ipAddress, port_num2);
                if(client2 == null)
                {
                    client2 = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                }
                client2.Connect(localEndPoint);

                // 设置收发缓存区，与模型训练侧要保持一致
                client2.ReceiveBufferSize = buf_size;
                client2.SendBufferSize = buf_size;
                // Debug.Log("GamebotSocket init");
            } else if (GamebotData.service_type == "FILE") {
            } else if (GamebotData.service_type == "SOCKET") {
            } else if (GamebotData.service_type == "LIBRARY") {
            /*
                string modelPath = Path.Combine(Application.streamingAssetsPath, modelFileName);
                interpreter = new Interpreter(FileUtil.LoadFile(modelFileName));
                if (interpreter == null)
                {
                    Debug.Log("interpreter is null");
                    return;
                }

                if (inputs == null)
                {
                    Debug.Log("Inputs is null");
                    return;
                }

                if (outputs == null || outputs.Length != inputs.Length)
                {
                    interpreter.ResizeInputTensor(0, new int[] { inputs.Length });
                    interpreter.AllocateTensors();
                    outputs = new float[inputs.Length];
                }
                Debug.Log("TFLite  init");
                // 读入模型
                interpreter.SetInputTensorData(0, inputs);
                interpreter.Invoke();
                interpreter.GetOutputTensorData(0, outputs);
                for (int i = 0; i < outputs.Length; i++)
                {
                    Debug.Log(outputs[i]);
                }
                Debug.Log("TFLite  init");
                // End
            */
            } else {
            }
        }

        public static void Init()
        {
            if (GamebotData.service_type == "SOCKET_VS_BT" || GamebotData.service_type == "SOCKET_VS_MULTIAGENT")
            {
                GamebotData.reset_request = new RequestRestartGame();
                GamebotData.step_request = new RequestStep();
                GamebotData.step_request.Hero = new Hero();

            } else if (GamebotData.service_type == "SOCKET_VS_SOCKET") {
                GamebotData.game_quit = new RequestQuit();
                GamebotData.reset_request = new RequestRestartGame();
                GamebotData.step_request = new RequestStep();
                GamebotData.step_request.Hero = new Hero();

            } else if (GamebotData.service_type == "FILE") {
            } else if (GamebotData.service_type == "SOCKET") {
            } else if (GamebotData.service_type == "LIBRARY") {
            } else {
            }
        }

        public static void Reset()
        {
            if (GamebotData.service_type == "SOCKET_VS_BT" || GamebotData.service_type == "SOCKET_VS_MULTIAGENT")
            {
                // 发送初始化游戏状态
                GamebotData.game_request = new Request();
                GamebotData.game_request.RestartGame = GamebotData.reset_request;

                if(GamebotData.train)
                {
                    GamebotService.SocketSend(client1, GamebotData.game_request);
                    // 接受传回来的动作或者游戏退出的请求
                    GamebotData.game_response1 = GamebotService.SocketResponse(client1);

                    // 如果请求是退出则退出游戏
                    switch (GamebotData.game_response1.ResponseCase)
                    {
                        case Response.ResponseOneofCase.Quit:
                            GamebotService.client1.Close();
                            Application.Quit();
                            break;
                        default:
                            break;
                    }
                }
            } else if (GamebotData.service_type == "SOCKET_VS_SOCKET") {
                // 发送初始化游戏状态
                GamebotData.game_request = new Request();
                GamebotData.game_request.RestartGame = GamebotData.reset_request;
                GamebotService.SocketSend(client1, GamebotData.game_request);

                // 接受传回来的动作或者游戏退出的响应
                GamebotData.game_response1 = GamebotService.SocketResponse(client1);

                // 如果响应是退出则退出游戏
                switch (GamebotData.game_response1.ResponseCase)
                {
                    case Response.ResponseOneofCase.Quit:
                        // 向第二两个端口发送游戏结束指令
                        GamebotData.game_request = new Request();
                        GamebotData.game_request.Quit = GamebotData.game_quit;
                        GamebotService.SocketSend(client2, GamebotData.game_request);

                        GamebotService.client1.Close();
                        GamebotService.client2.Close();
                        Application.Quit();
                        break;
                    default:
                        break;
                }

                GamebotService.SocketSend(client2, GamebotData.game_request);
                GamebotData.game_response2 = GamebotService.SocketResponse(client2);

            } else if (GamebotData.service_type == "FILE") {
            } else if (GamebotData.service_type == "SOCKET") {
            } else if (GamebotData.service_type == "LIBRARY") {
            } else {
            }
        }

        public static void Step()
        {
            if (GamebotData.service_type == "SOCKET_VS_BT" || GamebotData.service_type == "SOCKET_VS_MULTIAGENT")
            {
                // 发送游戏状态
                GamebotData.game_request = new Request();
                GamebotData.game_request.Step = GamebotData.step_request;

                if(GamebotData.train)
                {
                    GamebotService.SocketSend(client1, GamebotData.game_request);
                    // 判断游戏是否结束
                    if(GamebotData.step_request.Done == true)
                    {
                        SceneManager.LoadScene("MainScene");
                        // Debug.Log("Restart Game...");
                    }
                    else
                    {
                        // 接受传回来的动作
                        GamebotData.game_response1 = GamebotService.SocketResponse(client1);
                    }
                }
                // 清空proto信息
                GamebotData.step_request.Hero = new Hero();

            } else if (GamebotData.service_type == "SOCKET_VS_SOCKET") {
                // 发送游戏状态
                GamebotData.game_request = new Request();
                GamebotData.game_request.Step = GamebotData.step_request;
                GamebotService.SocketSend(client1, GamebotData.game_request);

                // 判断游戏是否结束
                if(GamebotData.step_request.Done == true)
                {
                    SceneManager.LoadScene("MainScene");
                    // Debug.Log("Restart Game...");
                }
                else
                {
                    // 如果游戏没有结束再向第二个socket发送信息
                    GamebotService.SocketSend(client2, GamebotData.game_request);

                    // 接受传回来的动作
                    GamebotData.game_response1 = GamebotService.SocketResponse(client1);
                    GamebotData.game_response2 = GamebotService.SocketResponse(client2);
                }
                GamebotData.step_request.Hero = new Hero();

            } else if (GamebotData.service_type == "FILE") {
            } else if (GamebotData.service_type == "SOCKET") {
            } else if (GamebotData.service_type == "LIBRARY") {
            /*
                // TF-lite for mnist: Start
                interpreter.SetInputTensorData(0, inputs);
                interpreter.Invoke();
                interpreter.GetOutputTensorData(0, outputs);
                for (int i = 0; i < outputs.Length; i++)
                {
                    Debug.Log(outputs[i]);
                }
                GamebotData.game_response1.Step.Player[0].Jump = 1;
            */
            } else {
            }
        }

        // socket发送
        public static void SocketSend(Socket client, Request request)
        {
            // 包的格式：len+data。len为4字节int，big-endian；data为protobuf数据内容
            byte[] request_buf = request.ToByteArray();
            int request_len = request_buf.Length;
            byte[] len_buf = BitConverter.GetBytes(request_len);
            byte[] send_buf = new byte[len_buf.Length + request_buf.Length];
            if (BitConverter.IsLittleEndian)
                Array.Reverse(len_buf);
            len_buf.CopyTo(send_buf, 0);
            request_buf.CopyTo(send_buf, len_buf.Length);
            int real_send = client.Send(send_buf, send_buf.Length, SocketFlags.None);
        }

        // socket接收
        public static Response SocketResponse(Socket client)
        {
            bytes = new Byte[4];
            client.Receive(bytes, 4, SocketFlags.None);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            int length = BitConverter.ToInt32(bytes, 0);
            bytes = new Byte[length];
            client.Receive(bytes, length, SocketFlags.None);
            Response temp_response = Response.Parser.ParseFrom(bytes, 0, length);
            return temp_response;
        }
    }
}
