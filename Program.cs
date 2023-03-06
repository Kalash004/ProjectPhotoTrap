namespace ProjectPhotoTrap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("working");
            SerialPortConnection port = new SerialPortConnection("COM4",115200,1000);
            Manager manager = new Manager();
            port.AddSub(manager);
            Thread thread = new Thread(new ThreadStart(port.StartReading));
            thread.Start();
        }
    }
}