using System.Text.Json.Serialization;

namespace HeatingDataMonitor.Notifications;

internal class SignalMessageModel
{
    /// <summary>
    /// Number to send from.
    /// </summary>
    [JsonPropertyName("number")]
    public string Number { get; set; }

    /// <summary>
    /// The recipients of the message. Can contain numbers and group-ids.
    /// </summary>
    [JsonPropertyName("recipients")]
    public ICollection<string> Recipients { get; set; }

    /// <summary>
    /// Message to send. Can be styled with markdown if TextMode is "styled".
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }

    /// <summary>
    /// Text mode of the message. Either "normal" or "styled".
    /// </summary>
    [JsonPropertyName("text_mode")]
    public string TextMode { get; set; }
}
