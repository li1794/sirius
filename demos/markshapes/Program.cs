﻿/*
 *                                                            ,--,      ,--,                              
 *             ,-.----.                                     ,---.'|   ,---.'|                              
 *   .--.--.   \    /  \     ,---,,-.----.      ,---,       |   | :   |   | :      ,---,           ,---,.  
 *  /  /    '. |   :    \ ,`--.' |\    /  \    '  .' \      :   : |   :   : |     '  .' \        ,'  .'  \ 
 * |  :  /`. / |   |  .\ :|   :  :;   :    \  /  ;    '.    |   ' :   |   ' :    /  ;    '.    ,---.' .' | 
 * ;  |  |--`  .   :  |: |:   |  '|   | .\ : :  :       \   ;   ; '   ;   ; '   :  :       \   |   |  |: | 
 * |  :  ;_    |   |   \ :|   :  |.   : |: | :  |   /\   \  '   | |__ '   | |__ :  |   /\   \  :   :  :  / 
 *  \  \    `. |   : .   /'   '  ;|   |  \ : |  :  ' ;.   : |   | :.'||   | :.'||  :  ' ;.   : :   |    ;  
 *   `----.   \;   | |`-' |   |  ||   : .  / |  |  ;/  \   \'   :    ;'   :    ;|  |  ;/  \   \|   :     \ 
 *   __ \  \  ||   | ;    '   :  ;;   | |  \ '  :  | \  \ ,'|   |  ./ |   |  ./ '  :  | \  \ ,'|   |   . | 
 *  /  /`--'  /:   ' |    |   |  '|   | ;\  \|  |  '  '--'  ;   : ;   ;   : ;   |  |  '  '--'  '   :  '; | 
 * '--'.     / :   : :    '   :  |:   ' | \.'|  :  :        |   ,/    |   ,/    |  :  :        |   |  | ;  
 *   `--'---'  |   | :    ;   |.' :   : :-'  |  | ,'        '---'     '---'     |  | ,'        |   :   /   
 *             `---'.|    '---'   |   |.'    `--''                              `--''          |   | ,'    
 *               `---`            `---'                                                        `----'   
 * 
 *
 * 
 * IRtc 인터페이스를 직접 사용하는 방법
 * RTC5 카드를 초기화 하고 원, 사각형, 도트 원 을 그린다
 * Author : hong chan, choi / sepwind @gmail.com(https://sepwind.blogspot.com)
 * 
 */

using System;
using System.Diagnostics;
using System.Numerics;

namespace SpiralLab.Sirius
{

    class Program
    {
        static void Main(string[] args)
        {
            SpiralLab.Core.Initialize();

            #region initialize RTC 
            IRtc rtc = new RtcVirtual(0); ///가상 rtc 제어기 생성
            //IRtc rtc = new Rtc5(0); ///rtc 5 제어기 생성
            double fov = 60.0;    /// scanner field of view : 60mm            
            double kfactor = Math.Pow(2, 20) / fov; /// k factor (bits/mm) = 2^20 / fov
            rtc.Initialize(kfactor, LaserMode.Yag1, "cor_1to1.ct5");    /// 스캐너 보정 파일 지정 : correction file
            rtc.CtlFrequency(50 * 1000, 2); /// laser frequency : 50KHz, pulse width : 2usec
            rtc.CtlSpeed(100, 100); /// default jump and mark speed : 100mm/s
            rtc.CtlDelay(10, 100, 200, 200, 0); /// scanner and laser delays
            #endregion

            #region initialize Laser (virtial)
            ILaser laser = new LaserVirtual(0, "virtual", 20);
            #endregion

            ConsoleKeyInfo key;
            do
            {
                Console.WriteLine("Testcase for spirallab.sirius. powered by labspiral@gmail.com (https://sepwind.blogspot.com)");
                Console.WriteLine("");                
                Console.WriteLine("'C' : draw circle");
                Console.WriteLine("'R' : draw rectangle");
                Console.WriteLine("'D' : draw circle with dots");
                Console.WriteLine("'Q' : quit");
                Console.WriteLine("");
                Console.Write("select your target : ");
                key = Console.ReadKey(false);
                if (key.Key == ConsoleKey.Q)
                    break;
                Console.WriteLine("\r\nWARNING !!! LASER IS BUSY ...");
                var timer = new Stopwatch();
                switch (key.Key)
                {
                    case ConsoleKey.C: 
                        DrawCircle(laser, rtc, 10);
                        break;
                    case ConsoleKey.R:
                        DrawRectangle(laser, rtc, 10, 10);
                        break;
                    case ConsoleKey.D:
                        DrawCircleWithDots(laser, rtc, 10, 1.0);
                        break;
                }
                rtc.ListExecute(true);
                Console.WriteLine($"processing time = {timer.ElapsedMilliseconds/1000.0:F3}s");                
            } while (true);

            rtc.Dispose();
        }        
        /// <summary>
        /// 지정된 반지름을 갖는 원 그리기
        /// </summary>
        /// <param name="rtc"></param>
        /// <param name="radius"></param>
        private static void DrawCircle(ILaser laser, IRtc rtc, double radius)
        {
            rtc.ListBegin(laser);
            rtc.ListJump(new Vector2((float)radius, 0));
            rtc.ListArc(new Vector2(0, 0), 360.0);
            rtc.ListEnd();
        }
        /// <summary>
        /// 지정된 크기의 직사각형 그리기
        /// </summary>
        /// <param name="rtc"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void DrawRectangle(ILaser laser, IRtc rtc, double width, double height)
        {
            rtc.ListBegin(laser);
            rtc.ListJump(new Vector2((float)-width / 2, (float)height / 2));
            rtc.ListMark(new Vector2((float)width / 2, (float)height / 2));
            rtc.ListMark(new Vector2((float)width / 2, (float)-height / 2));
            rtc.ListMark(new Vector2((float)-width / 2, (float)-height / 2));
            rtc.ListMark(new Vector2((float)-width / 2, (float)height / 2));
            rtc.ListEnd();
        }
        /// <summary>
        /// 원(호)를 따라 매 1도마다 점 찍기
        /// </summary>
        /// <param name="rtc"></param>
        /// <param name="radius"></param>
        /// <param name="durationMsec"></param>
        private static void DrawCircleWithDots(ILaser laser, IRtc rtc, double radius, double durationMsec)
        {
            rtc.ListBegin(laser);
            for (double angle=0; angle<360; angle+=1)
            {
                double x = radius * Math.Sin(angle * Math.PI / 180.0);
                double y = radius * Math.Cos(angle * Math.PI / 180.0);
                rtc.ListJump(new Vector2((float)x, (float)y));
                rtc.ListLaserOn(durationMsec);                
            }            
            rtc.ListEnd();
        }        
    }
}
