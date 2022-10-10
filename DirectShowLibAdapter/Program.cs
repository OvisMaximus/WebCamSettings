// See https://aka.ms/new-console-template for more information

namespace DirectShowLibAdapter;

class Program
{
    public static void Main(string[] args)
    {
        var deviceController = new DirectShowDeviceAdapterImpl();
        var devicesList = deviceController.GetCameraDevicesList();
        foreach (var cameraDevice in devicesList)
        {
            Console.WriteLine(cameraDevice.GetDeviceName());
            foreach(var property in cameraDevice.GetPropertiesList())
            {
                Console.WriteLine($"        {property}");
            }
        }
    }    
}

