using System.Text.Json;
using Microsoft.JSInterop;
using Radzen;

namespace BlazorServerApp.Services;

public class EventConsoleService
{
    public class Message
    {
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public AlertStyle AlertStyle { get; set; }
    }

    public bool IsVisible { get; set; }
    public IList<Message> messages = new List<Message>();
    
    public EventConsoleService( )
    {
        // IsVisible = true;
    }
    

    public void Clear() => messages.Clear();

    public void Log(string message, AlertStyle alertStyle = AlertStyle.Info)
    {
        messages.Add(new Message { Date = DateTime.Now, Text = message, AlertStyle = alertStyle });
        NewLogEvent?.Invoke(this,new EventArgs());
    }

    public void Log(object value)
    {
        Log(JsonSerializer.Serialize(value));
    }

    public event EventHandler? NewLogEvent;
}