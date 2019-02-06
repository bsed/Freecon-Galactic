using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using XNAColor = Microsoft.Xna.Framework.Color;

namespace Freecon.Client.CefSharpWrapper
{
    /// <summary>
    /// Credit to Kevin Gadd, the code below is taken from work detailed in his blog, with minor adaptation
    /// </summary>
    public static class TextureUtils
    {

        internal unsafe delegate int GetSurfaceLevelDelegate(void* pTexture, uint iLevel, void** pSurface);
        internal unsafe delegate uint ReleaseDelegate(void* pObj);

        /// <summary>
        /// Low level sugar to offset the pointer to an interface, so that a method can be called
        /// </summary>
        /// <param name="pInterface"></param>
        /// <param name="offsetInBytes"></param>
        /// <returns></returns>
        public static unsafe void* AccessVTable(void* pInterface, uint offsetInBytes)
        {

            void* pVTable = (*(void**)pInterface);

            return *((void**)((ulong)pVTable + offsetInBytes));

        }


        /// <summary>
        /// Pointer offsets to methods of COM interfaces
        /// </summary>
        public static class VTables
        {

            public static class IDirect3DTexture9
            {

                public const uint GetSurfaceLevel = 72;

            }

            public static class IUnknown
            {

                public const uint Release = 8;

            }

        }


        /// <summary>
        /// Reflects to get a pointer to the COM interface IDirect3dTexture9, then offsets the pointer to call GetSurfaceLevel, which returns a pointer to the Texture2D bit buffer
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static unsafe void* GetSurfaceLevel(this Texture2D texture, int level)
        {

            void* pTexture = GetIDirect3dTexture9(texture);

            void* pGetSurfaceLevel = AccessVTable(pTexture, VTables.IDirect3DTexture9.GetSurfaceLevel);

            void* pSurface;



            var getSurfaceLevel = (GetSurfaceLevelDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(pGetSurfaceLevel), typeof(GetSurfaceLevelDelegate));


            var rv = getSurfaceLevel(pTexture, 0, &pSurface);

            if (rv == 0)

                return pSurface;

            else

                throw new COMException("GetSurfaceLevel failed", rv);

        }


        /// <summary>
        /// Gets the IDirect3dTexture9 interface pointer by reflecting on the field container.pComPtr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        private static unsafe void* GetIDirect3dTexture9(Texture2D tex)
        {
            var dField = tex.GetType().GetField("pComPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            return Pointer.Unbox(dField.GetValue(tex));

        }

        /// <summary>
        /// Sets the buffer of texture to pSource (not sure if it copies memory or just assigns the pointer)
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="level"></param>
        /// <param name="pData">pointer to source memory buffer</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="pitch"></param>
        /// <param name="pixelFormat"></param>
        public static unsafe void SetData(this Texture2D texture, int level, void* pData, int width, int height, uint pitch, D3DFORMAT pixelFormat)
        {

            var rect = new RECT { Top = 0, Left = 0, Right = width, Bottom = height };
            void* pSurface = GetSurfaceLevel(texture, level);
            

            try
            {
                var rv = D3DXLoadSurfaceFromMemory(pSurface, null, &rect, pData, pixelFormat, pitch, null, &rect, D3DX_FILTER.NONE, 0);

                if (rv != 0)
                {
                    throw new COMException("D3DXLoadSurfaceFromMemory failed", rv);
                }
            }
            finally
            {
                Release(pSurface);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="level">I still don't know what this is, but it works when we use 0. heh.</param>
        /// <param name="pSource">pointer to source buffer</param>
        public static unsafe void SetDataDirectBufferCopy(this Texture2D texture, int level, void* pSource)
        {
            byte* pDestination = (byte*)GetSurfaceLevel(texture, level);
            int bufferSize = texture.Width* texture.Height * 4;//assuming 32 bit pixel size

            var pSourceBytes = (byte*) pSource;
           
            throw new Exception("Broken, because Texture2d uses compression, while this attempts a direct uncompressed copy. Read procteted memory error results.");

            for (int i = 0; i < bufferSize; i++)
            {
                pDestination[i] = pSourceBytes[i];
            }

        }
        public static unsafe void SetDataDirectBufferCopy(this Texture2D outTex, Bitmap bmp)
        {
            if (bmp.Width != outTex.Width || bmp.Height != outTex.Height)
            {
                throw new InvalidOperationException("outTex and bmp must have equal dimensions.");
            }

            // lock bitmap
            BitmapData origdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            void* bufferPointer = (void*)origdata.Scan0;

            outTex.SetDataDirectBufferCopy(0, bufferPointer);

            bmp.UnlockBits(origdata);

        }


        public static unsafe void SetData(this Texture2D outTex, Bitmap bmp)
        {
            if (bmp.Width != outTex.Width || bmp.Height != outTex.Height)
            {
                throw new InvalidOperationException("outTex and bmp must have equal dimensions.");
            }

            // lock bitmap
            BitmapData origdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

            void* bufferPointer = (void*)origdata.Scan0;

            outTex.SetData(0, bufferPointer, bmp.Width, bmp.Height, (uint)(bmp.Width * sizeof(XNAColor)), D3DFORMAT.A8R8G8B8);

            bmp.UnlockBits(origdata);

        }

        /// <summary>
        /// Copies the buffer from inTex to the buffer of outTex
        /// if inTex and outTex are not the same size, an InvalidOperationException is thrown
        /// </summary>
        /// <param name="inTex"></param>
        /// <param name="outTex"></param>
        public static unsafe void SetData(this Texture2D outTex, Texture2D inTex)
        {
            if (inTex.Width != outTex.Width || inTex.Height != outTex.Height)
            {
                throw new InvalidOperationException("inTex and outTex must be the same size.");
            }

            //Texture2D.GetData simply requires an array to copy data to. 
            //sizeof(T) must equal the size (in bytes) of the pixel format in the buffer.  
            //XNA default format appears to be SurfaceFormat.Color, not sure if there's an easy way to go from any Texture2D.Format to an appropriate T
            //Throw for now
            if (inTex.Format != SurfaceFormat.Color)
            {
                throw new InvalidOperationException("SetData is only implemented for textures with Format==SurfaceFormat.Color");
            }
            XNAColor[] data = new XNAColor[inTex.Width * inTex.Height];
            inTex.GetData<XNAColor>(data);


            //fixed prevents the garbage collector from moving the value pointed to the assigned pointer
            fixed (void* dataPtr = &data[0])
            {
                outTex.SetData(0, dataPtr, inTex.Width, inTex.Height, (uint)(inTex.Width * sizeof(XNAColor)), D3DFORMAT.A8R8G8B8);

            }


        }

        public static void SetDataMidLevel(this Texture2D texture, Bitmap bmp)
        {
            if (texture.Width != bmp.Width || texture.Height != bmp.Height)
                throw new InvalidOperationException("texture must be the same height and width of the bitmap.");


            int[] imgData = new int[bmp.Width * bmp.Height];


            unsafe
            {
                // lock bitmap
                BitmapData origdata =
                    bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                uint* byteData = (uint*)origdata.Scan0;

                // Switch bgra -> rgba
                for (int i = 0; i < imgData.Length; i++)
                {
                    byteData[i] = (byteData[i] & 0x000000ff) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }

                // copy data
                Marshal.Copy(origdata.Scan0, imgData, 0, bmp.Width * bmp.Height);

                byteData = null;

                // unlock bitmap
                bmp.UnlockBits(origdata);
            }

            texture.SetData(imgData);

        }

        public static void SetDataSlow(out Texture2D texture, Bitmap bmp, GraphicsDevice device)
        {
            using (MemoryStream s = new MemoryStream())
            {
                bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                s.Seek(0, SeekOrigin.Begin);
                texture = Texture2D.FromStream(device, s);
            }

        }

        public static unsafe uint Release(void* pObj)
        {

            void* pRelease = AccessVTable(pObj, VTables.IUnknown.Release);
            var release = (ReleaseDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(pRelease), typeof(ReleaseDelegate));
            return release(pObj);

        }

        [DllImport("d3dx9_41.dll")]
        private static unsafe extern int D3DXLoadSurfaceFromMemory(void* pDestSurface, void* pDestPalette, RECT* pDestRect, void* pSrcMemory, D3DFORMAT srcFormat, uint srcPitch, void* pSrcPalette, RECT* pSrcRect, D3DX_FILTER filter, uint colorKey);


    }


    public enum D3DFORMAT : uint
    {

        UNKNOWN = 0,



        R8G8B8 = 20,

        A8R8G8B8 = 21,

        X8R8G8B8 = 22,

        A8B8G8R8 = 32,

        X8B8G8R8 = 33,

    }

    [Flags]
    public enum D3DX_FILTER : uint
    {

        DEFAULT = 0xFFFFFFFF,

        NONE = 0x00000001,

        POINT = 0x00000002,

        LINEAR = 0x00000003,

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {

        public int Left;

        public int Top;

        public int Right;

        public int Bottom;

    }
}
