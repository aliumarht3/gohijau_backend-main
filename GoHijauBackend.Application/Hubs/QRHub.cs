using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class QRHub : Hub
{
    public async Task JoinTokenGroup(string token)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, token);
    }
    public async Task SendStatus(string machineId, string status)
    {

        try
        {
            // Build the file path
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "testwrite");
            var logFilePath = Path.Combine(logDirectory, "status_log.txt");

            // Ensure the folder exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Prepare the message
            var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Status from {machineId}: {status}{Environment.NewLine}";

            // Append the message to the file asynchronously
            await File.AppendAllTextAsync(logFilePath, logMessage);

            // Optionally, broadcast the status via SignalR
            await Clients.All.SendAsync("ReceiveStatus", machineId, status);
        }
        catch (Exception ex)
        {
            // Log any file write errors
            Console.WriteLine($"Error writing status log: {ex.Message}");
        }
    }
}
