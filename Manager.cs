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

     
        private static String PATTERN_FOR_IP = @"http:\/\/\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}";

        private static String TURN_ON_FACE_DETECTION = @"/control?var=face_detect&val=";
        private static String TURN_ON_STREAM = ":81/stream";
        private static String GET_IMAGE = @"/capture";
        private String link = null;

        private static String namingPattern = "FaceDetected";
        private static String endingPattern = ".jpeg";
        private static String savePath = @"../../../Photos/";
        private static int photo_id = 0;

        private bool isStreaming = false;
        private static int SAVE_FACE_SLEEP = 10000;
        private bool madePhoto = false;


        public void DataRead(object sender, string message)
        {
            throw new NotImplementedException("This is deprecated");
        }
        public void DataRead(object sender, string message, Type type)
        {
            if (type == Type.FaceDetected)
            {
                Console.WriteLine("Face");
                if (!madePhoto)
                {
                    SaveFace();
                    madePhoto = true;
                } else
                {
                    Thread.Sleep(SAVE_FACE_SLEEP);
                    madePhoto = false;
                }
            }
            else if (type == Type.ServerLink)
            {
                this.link = ObtainLink(message);
                if (link == null)
                {
                    throw new Exception("Camera didnt send link correctly, please restart program and camera"); // CATCH ME: chytat a automaticky zacit programu znovu
                }
                if (!isStreaming) TurnOnStream(); isStreaming = true;
                SetFaceDet();
            }
        }
        private void TurnOnStream()
        {
            Thread thread = new Thread(new ThreadStart(ThreadStream));
            thread.Start();
        }
        private void ThreadStream()
        {
            while (true)
            {
                Console.WriteLine("Threading");
                if (link == null)
                {
                    throw new Exception("Link is null, please restart the camera");
                }
                var web_request = WebRequest.Create(link + TURN_ON_STREAM);
                web_request.GetResponse();
            }
        }
        private async void SaveFace()
        {
            var format = "MM-dd-yyyy-hh-mm-ss";
            var time = DateTime.Now;
            var path = savePath+ time.ToString(format) + namingPattern + photo_id.ToString() + endingPattern;
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(link + GET_IMAGE);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                SaveFace();
                return;
            }
            if (!response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
            {
                SaveFace();
                return;
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
        private void SetFaceDet()
        {
            String pattern = TURN_ON_FACE_DETECTION;
            pattern = pattern + "1";
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
       
    }
}
