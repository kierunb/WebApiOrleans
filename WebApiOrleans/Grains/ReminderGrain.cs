// https://learn.microsoft.com/en-us/dotnet/orleans/grains/timers-and-reminders

namespace WebApiOrleans.Grains
{
    public interface IReminderGrain : IGrainWithStringKey, IRemindable
    {
        Task StartMessage();
        Task StopMessage();
    }

    public class ReminderGrain(ILogger<ReminderGrain> logger) : Grain, IReminderGrain
    {
        const string ReminderName = "reminderMessage";

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            //Determine if it matches the name of the reminder
            if (reminderName == ReminderName)
            {
                logger.LogInformation("Reminder message created at: {date}", DateTime.Now);
            }
            return Task.CompletedTask;
        }

        public Task StartMessage()
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
