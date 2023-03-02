using Orleans.Runtime;

// https://learn.microsoft.com/en-us/dotnet/orleans/grains/timers-and-reminders

namespace WebApiOrleans.Grains
{
    public interface IReminderGrain : IGrainWithStringKey, IRemindable
    {
        Task SendMessage();
        Task StopMessage();
    }

    public class ReminderGrain : Grain, IReminderGrain
    {
        const string ReminderName = "reminderMessage";
        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            //Determine if it matches the name of the reminder
            if (reminderName == ReminderName)
            {
                Console.WriteLine($"Reminder message created at: {DateTime.Now}");
            }
            return Task.CompletedTask;
        }

        public Task SendMessage()
        {
            
            this.RegisterOrUpdateReminder(ReminderName, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public async Task StopMessage()
        {
            foreach (var reminder in await this.GetReminders())
            {
                if (reminder.ReminderName == ReminderName)
                {
                    await this.UnregisterReminder(reminder);
                }
            }
        }
    }
}
