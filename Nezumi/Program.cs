using Windows.Win32;
using Windows.Win32.Foundation;
using Nezumi;

Console.WriteLine("Press any button to start the task");
Console.ReadKey();

var task = new BackgroundTask(TimeSpan.FromMilliseconds(1));
task.Start();

Console.WriteLine("Press any button to stop the task");
Console.ReadKey();

await task.StopAsync();