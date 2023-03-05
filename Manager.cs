using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProjectPhotoTrap
{
    internal class Manager : IPortMessage
    {

        //D:\Documents\csharp\Projects\PhotoTrap\ProjectPhotoTrap\ProjectPhotoTrap\Photos\
        // 3 back
        private static String PATTERN_FOR_IP = @"http:\/\/\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";

        private static String TURN_ON_FACE_DETECTION = @"/control?var=face_detect&val=";
        private static String TURN_ON_STREAM = ":81/stream";
        private static String GET_IMAGE = @"/capture";
        private String link = null;

        private static String namingPattern = "FaceDetected";
        private static String endingPattern = ".jpeg";
        private static String savePath = @"../../../Photos/";
        private static int photo_id = 0;





        public void DataRead(object sender, string message)
        {
            Console.WriteLine("reading");
            Console.WriteLine(message);
        }
        public void DataRead(object sender, string message, Type type)
        {
            if (type == Type.FaceDetected)
            {
                Console.WriteLine("Face");
                SaveFace();
            }
            else if (type == Type.ServerLink)
            {
                this.link = ObtainLink(message);
                TurnOnStream();
                SetFaceDet(1);
            }

        }
        // /stream
        private void TurnOnStream()
        {
            Thread thread = new Thread(ThreadStream);
            thread.Start();
        }

        private void ThreadStream()
        {
            while (true)
            {
                Console.WriteLine("Threading");
                var web_request =WebRequest.Create(link+TURN_ON_STREAM);
                web_request.GetResponse();
            }
        }

        private async void SaveFace()
        {
            var path = savePath + namingPattern + photo_id.ToString() + endingPattern;
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(link + GET_IMAGE);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Failed to download image. Status code: {response.StatusCode}");
            }
            if (!response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
            {
                throw new Exception("The URL does not contain an image.");
            }
            while (File.Exists(path))
            {
                photo_id++;
                path = savePath + namingPattern + photo_id.ToString() + endingPattern;
            }
            using var fileStream = File.Create(path);
            using var responseStream = await response.Content.ReadAsStreamAsync();
            await responseStream.CopyToAsync(fileStream);
        }

        private void SetFaceDet(int number)
        {
            string pattern = TURN_ON_FACE_DETECTION;
            if (number == 0)
            {
                // turn off face recognition
                pattern = pattern + "0";
            }
            else if (number == 1)
            {
                pattern = pattern + "1";
            }
            else
            {
                throw new Exception("To turn on face detection write 1 to turn off write 0, you wrote " + number);
            }
            WebRequest web_request = WebRequest.Create(link + pattern);
            web_request.Method = "GET";
            web_request.GetResponse();
        }



        private String ObtainLink(String str)
        {
            String link = null;
            Match match = Regex.Match(str, PATTERN_FOR_IP);
            if (match.Success)
            {
                link = match.Value;
                Console.WriteLine(link);
            }
            return link;
        }
        //private bool SavePhoto()
        //{

        //}   

    }
}
