using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFmpegDemo
{
    public partial class frmPlayer : Form
    {
        public frmPlayer()
        {
            InitializeComponent();
        }
        tstRtmp rtmp = new tstRtmp();
        Thread thPlayer;
        private void btnStart_Click(object sender, EventArgs e)
        {
            sdlPlay();
            //btnStart.Enabled = false;
            //if (thPlayer != null)
            //{
            //    rtmp.Stop();

            //    thPlayer = null;
            //}
            //else
            //{
            //    thPlayer = new Thread(DeCoding);
            //    thPlayer.IsBackground = true;
            //    thPlayer.Start();
            //    btnStart.Text = "停止播放";
            //    btnStart.Enabled = true;
            //}
        }

        /// <summary>
        /// 播放线程执行方法
        /// </summary>
        private unsafe void DeCoding()
        {
            try
            {
                Console.WriteLine("DeCoding run...");
                Bitmap oldBmp = null;


                // 更新图片显示
                tstRtmp.ShowBitmap show = (bmp) =>
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        this.pic.Image = bmp;
                        if (oldBmp != null)
                        {
                            oldBmp.Dispose();
                        }
                        oldBmp = bmp;
                    }));
                };
                rtmp.Start(show, txtUrl.Text.Trim());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine("DeCoding exit");
                rtmp.Stop();

                thPlayer = null;
                this.Invoke(new MethodInvoker(() =>
                {
                    btnStart.Text = "开始播放";
                    btnStart.Enabled = true;
                }));
            }
        }
        int count = 0;
        IntPtr screen;
        IntPtr sdlRenderer;
        IntPtr sdlTexture;
        SDL2.SDL.SDL_Rect sdlRect;
        SDL2.SDL.SDL_Rect sdlRect1;
        int screen_w = 640, screen_h = 360;
        const int pixel_w = 640, pixel_h = 360;
        char[] buffer = new char[640 * 360 * 12 / 8];
        uint pixformat= SDL2.SDL.SDL_PIXELFORMAT_IYUV;  
        public unsafe void sdlPlay()
        {
            //char a = (char)257;
            SDL2.SDL.SDL_Init(SDL2.SDL.SDL_INIT_VIDEO);
            screen = SDL2.SDL.SDL_CreateWindow("Simplest Video Play SDL2", SDL2.SDL.SDL_WINDOWPOS_UNDEFINED, SDL2.SDL.SDL_WINDOWPOS_UNDEFINED,
        screen_w, screen_h, SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL2.SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            sdlRenderer = SDL2.SDL.SDL_CreateRenderer(screen, -1, 0);
            sdlTexture = SDL2.SDL.SDL_CreateTexture(sdlRenderer, pixformat, (int)SDL2.SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, pixel_w, pixel_h);

            StreamReader sr = new StreamReader("./sintel_640_360.yuv");
            while (true)
            {
                count++;
                if (sr.Read(buffer, 0, buffer.Length) != pixel_w * pixel_h * 12 / 8)
                {
                    Console.WriteLine(count);
                    break;
                }

                GCHandle hObject = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                IntPtr pObject = hObject.AddrOfPinnedObject();
                sdlRect.x = 0;
                sdlRect.y = 0;
                sdlRect.w = screen_w;
                sdlRect.h = screen_h;
                SDL2.SDL.SDL_UpdateTexture(sdlTexture, ref sdlRect, pObject, pixel_w);
                
                

                SDL2.SDL.SDL_RenderClear(sdlRenderer);
                SDL2.SDL.SDL_RenderCopy(sdlRenderer, sdlTexture, (IntPtr)null, ref sdlRect);
                SDL2.SDL.SDL_RenderPresent(sdlRenderer);
                //Delay 40ms
                SDL2.SDL.SDL_Delay(40);
                if (hObject.IsAllocated)
                    hObject.Free();
            }
            SDL2.SDL.SDL_Quit();
            //return ;
        }
    }
}
