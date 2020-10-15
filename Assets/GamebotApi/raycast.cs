using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using GamebotData = bytedance.lab.gamebot.GamebotData;

public class raycast : MonoBehaviour
{
    public Camera weaponCamera;

    // 计算角度细分
    int h_num = 18;
    int w_num = 12;
    float h_angle = 30;
    float w_angle = 45;

    float distance = 10;
    // int num = 5; 
    RaycastHit hit;

    void ray_cast(float a, float b)
    {
        Vector3 pos = new Vector3(Mathf.Tan(a * Mathf.Deg2Rad) * distance, Mathf.Tan(b * Mathf.Deg2Rad) * distance, (float)10);
        Vector3 pos_center = new Vector3(0, 0, 10);
        pos = weaponCamera.transform.forward * distance + (pos - pos_center);

        if(Physics.Raycast(weaponCamera.transform.position, pos, out hit, Mathf.Infinity))
        {       
            // Debug.Log("碰撞对象: " + hit.collider.name);  
            // Debug.DrawLine(weaponCamera.transform.position, hit.point, Color.red); 
            switch(hit.collider.name)
            {
                case "Mesh":    GamebotData.step_request.Hero.RayCast.Add(2);   break;
                case "Loot_Health(Clone)": GamebotData.step_request.Hero.RayCast.Add(3);   break;
                case "Loot_Health": GamebotData.step_request.Hero.RayCast.Add(3);   break;
                case "HitBox":  GamebotData.step_request.Hero.RayCast.Add(4);   break;
                case "Player":  GamebotData.step_request.Hero.RayCast.Add(1);   break;
                // default:  print(hit.collider.name); break;
            }
            GamebotData.step_request.Hero.RayCast.Add(hit.distance);
        }
        else
        {
            //Debug.Log("没碰到");  
            GamebotData.step_request.Hero.RayCast.Add(0);
            GamebotData.step_request.Hero.RayCast.Add(0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float h_single = h_angle/h_num;
        float w_single = w_angle/h_num;
        /*
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start(); //  开始监视代码运行时间
        */
        GamebotData.step_request.Hero.RayCast.Clear();
        // Debug.Log( ((int)(h_angle/num) * 2 + 1) * ((int)(w_angle/num) * 2 + 1) );
        /*
        for(int i = (int)(-w_angle/num) ; i <= (int)(w_angle/num); ++i)
        {
            for(int j = (int)(-h_angle/num); j <= (int)(h_angle/num); ++j)
            {
                ray_cast(i * num, j * num);
            }
        }
        */

        for(int i = -w_num; i <= w_num; ++i)
        {
            for(int j = -h_num; j <= h_num; ++j)
            {
                ray_cast(i * w_single, j * h_single);
            }
        }
        // Debug.Log(GamebotData.step_request.Hero.RayCast.Count.ToString());

        /*
        stopwatch.Stop(); //  停止监视
        System.TimeSpan timespan = stopwatch.Elapsed;
        double milliseconds = timespan.TotalMilliseconds;
        Debug.Log(milliseconds);
        */

        // print(weaponCamera.transform.rotation.eulerAngles);
        GamebotData.step_request.Hero.X = weaponCamera.transform.position.x;
        GamebotData.step_request.Hero.Y = weaponCamera.transform.position.y;
        GamebotData.step_request.Hero.Z = weaponCamera.transform.position.z;
        GamebotData.step_request.Hero.RotationX = weaponCamera.transform.rotation.eulerAngles.x;
        GamebotData.step_request.Hero.RotationY = weaponCamera.transform.rotation.eulerAngles.y;
        GamebotData.step_request.Hero.RotationZ = weaponCamera.transform.rotation.eulerAngles.z;
    }
}
