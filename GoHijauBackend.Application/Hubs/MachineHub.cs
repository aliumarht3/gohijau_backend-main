using GoHijauBackend.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace GoHijauBackend.Application.Hubs
{
    public class MachineHub : Hub
    {
        private static readonly Dictionary<string, string> MachineConnections = new();

        public override Task OnConnectedAsync()
        {
            var machineId = Context.GetHttpContext()?.Request.Query["machineId"];
            if (!string.IsNullOrEmpty(machineId))
            {
                MachineConnections[machineId!] = Context.ConnectionId;
                Console.WriteLine($"Machine connected: {machineId}");
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var machine = MachineConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(machine.Key))
            {
                MachineConnections.Remove(machine.Key);
                Console.WriteLine($"Machine disconnected: {machine.Key}");
            }
            return base.OnDisconnectedAsync(exception);
        }
        private static string MachineGroup(string machineId) => $"machine:{machineId}";

        // ----------- Subscriptions for apps (collectors/admin) -----------

        /// Call from apps to start receiving updates for one machine
        public Task SubscribeMachine(string machineId)
            => Groups.AddToGroupAsync(Context.ConnectionId, MachineGroup(machineId));

        /// Call from apps to stop receiving updates for one machine
        public Task UnsubscribeMachine(string machineId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, MachineGroup(machineId));

        /// Bulk subscribe (collector with many machines)
        public Task SubscribeMachines(IEnumerable<string> machineIds)
            => Task.WhenAll(machineIds.Select(id => Groups.AddToGroupAsync(Context.ConnectionId, MachineGroup(id))));

        // Called by the machine to send status updates
        public async Task SendStatus(string machineId, string status)
        {

            try
            {
                // Optionally, broadcast the status via SignalR
                await Clients.All.SendAsync("ReceiveStatus", machineId, status);
            }
            catch (Exception ex)
            {
                // Log any file write errors
                Console.WriteLine($"Error writing status log: {ex.Message}");
            }
        }

        // Called by the server or admin panel to send commands
        public async Task SendCommand(string machineId, string command)
        {
            if (MachineConnections.TryGetValue(machineId, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveCommand", command);
            }
        }
        public async Task UpdateVolume(MachineUCOTracking payload)
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.MachineId))
                return;

            // Shape matches your RN listener: ReceiveMachineVolumeUpdate(Partial<MachineVolume>)
            var outbound = new
            {
                id = payload.Id,
                machineId = payload.MachineId,
                bufferVolume = payload.BufferVolume,
            };

            // Send to subscribers of that machine + (optionally) to all admins
            await Clients.Group(MachineGroup(payload.MachineId))
                .SendAsync("ReceiveMachineVolumeUpdate", outbound);

            // If you still want global feed for dashboards, uncomment:
            // await Clients.All.SendAsync("ReceiveMachineVolumeUpdate", outbound);
        }
    }
}
