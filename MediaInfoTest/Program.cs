using System;
using System.Collections.Generic;

using System.Text;
using MediaInfoLib;
using System.IO;

namespace MediaInfoTest
{
    class Program
    {
        //Example.ogg o SuperMario64DireDireDocks.mp3
        public static string FilePath = "";

        static void Main(string[] args)
        {
            FileMediaInformation();
        }

        public static void FileMediaInformation() {

            Console.WriteLine("Introduzca una ruta:");
            FilePath = Console.ReadLine();

            if (File.Exists(FilePath))
            {
                MediaInfo _info = new MediaInfo();

                Console.WriteLine(_info.Option("Info_Version", "0.7.0.0;MediaInfoDLL_Example_CS;0.7.0.0"));

                _info.Open(FilePath);

                //informacion del archivo, tipo de archivo, filesize, etc...
                _info.Option("Complete");
                Console.WriteLine(_info.Inform());

                Console.WriteLine("\n----------------------------------");

                //file size
                _info.Option("Inform", "General;File size is %FileSize% bytes");
                Console.WriteLine(_info.Inform());

                Console.WriteLine("\n----------------------------------");

                Console.WriteLine(_info.Count_Get(StreamKind.Audio));

                Console.WriteLine("\n----------------------------------");

                Console.WriteLine(_info.Get(StreamKind.General, 0, "AudioCount"));

                Console.WriteLine("\n----------------------------------");

                Console.WriteLine(_info.Get(StreamKind.Audio, 0, "StreamCount"));

                Console.WriteLine("\n----------------------------------");

                //tipo de formato del archivo
                Console.WriteLine(_info.Get(StreamKind.General, 0, "Format"));

                Console.WriteLine("\n----------------------------------");

                Console.WriteLine(_info.Get(StreamKind.Audio, 0, "BitRate"));

                Console.WriteLine("\n----------------------------------");

                Console.WriteLine(ExampleWithStream());

                do
                {
                    Console.WriteLine("\n----------------------------------");
                    Console.WriteLine("Introduzca un comando para StreamKind.Audio:");
                    Console.WriteLine(_info.Get(StreamKind.Audio, 0, Console.ReadLine()));
                    Console.WriteLine("\n----------------------------------");
                    Console.WriteLine("\nDesea Continuar? Y/N");
                }
                while (Console.ReadLine().ToUpper() != "N");


                Console.ReadLine();
            }
            else {
                Console.WriteLine("El archivo no existe. Desea Continuar? Y/N");
                if (Console.ReadLine().ToUpper() != "N")
                {
                    FileMediaInformation();
                }
            }
        }

        public static String ExampleWithStream()
        {
            //Initilaizing MediaInfo
            MediaInfo MI = new MediaInfo();

            //From: preparing an example file for reading
            FileStream From = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            //From: preparing a memory buffer for reading
            byte[] From_Buffer = new byte[64 * 1024];
            int From_Buffer_Size; //The size of the read file buffer

            //Preparing to fill MediaInfo with a buffer
            MI.Open_Buffer_Init(From.Length, 0);

            //The parsing loop
            do
            {
                //Reading data somewhere, do what you want for this.
                From_Buffer_Size = From.Read(From_Buffer, 0, 64 * 1024);

                //Sending the buffer to MediaInfo
                System.Runtime.InteropServices.GCHandle GC = System.Runtime.InteropServices.GCHandle.Alloc(From_Buffer, System.Runtime.InteropServices.GCHandleType.Pinned);
                IntPtr From_Buffer_IntPtr = GC.AddrOfPinnedObject();
                Status Result = (Status)MI.Open_Buffer_Continue(From_Buffer_IntPtr, (IntPtr)From_Buffer_Size);
                GC.Free();
                if ((Result & Status.Finalized) == Status.Finalized)
                    break;

                //Testing if MediaInfo request to go elsewhere
                if (MI.Open_Buffer_Continue_GoTo_Get() != -1)
                {
                    Int64 Position = From.Seek(MI.Open_Buffer_Continue_GoTo_Get(), SeekOrigin.Begin); //Position the file
                    MI.Open_Buffer_Init(From.Length, Position); //Informing MediaInfo we have seek
                }
            }
            while (From_Buffer_Size > 0);

            //Finalizing
            MI.Open_Buffer_Finalize(); //This is the end of the stream, MediaInfo must finnish some work

            //Get() example
            return "Container format is " + MI.Get(StreamKind.General, 0, "Format");
        }

    }
}
