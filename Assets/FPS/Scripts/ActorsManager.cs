using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using GamebotData = bytedance.lab.gamebot.GamebotData;

public class ActorsManager : MonoBehaviour
{
    public List<Actor> actors { get; private set; }
    int max_num;
    bool first_frame = true;
    
    private void Awake()
    {
        actors = new List<Actor>();
    }
    private void Start()
    {
    }

    private void Update()
    {
        if(first_frame) 
        {
            max_num = actors.Count;
            first_frame = false;
        }
        else
        {
            GamebotData.step_request.Hero.Kill = max_num - actors.Count; 
            if(actors.Count == 1)
            {
                GamebotData.step_request.Done = true;
            }
        }
    }
}
