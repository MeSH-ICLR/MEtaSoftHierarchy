using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using GamebotService = bytedance.lab.gamebot.GamebotService;
using GamebotData = bytedance.lab.gamebot.GamebotData;
using bytedance.lab.gamebot;
using UnityEngine.SceneManagement;
using System;


public class AIbot : MonoBehaviour
{
    double t=0;
    void Awake()
    {
        GamebotService.Init();
    }

    void Start()
    {
        GamebotService.Reset();
    }

    void LateUpdate()
    {   

        if(GamebotData.now_frame == 0)  
        {
            print(Time.time - t);
            GamebotService.Step();
            t = Time.time;
        }
        GamebotData.now_frame = GamebotData.now_frame + 1;
        if(GamebotData.now_frame == GamebotData.frame) GamebotData.now_frame = 0;
    }
}
