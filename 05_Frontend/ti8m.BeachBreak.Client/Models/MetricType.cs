namespace ti8m.BeachBreak.Client.Models
{
    /// <summary>
    /// Defines the visual theme/color for MetricCard components
    /// </summary>
    public enum MetricType
    {
        /// <summary>
        /// Default/neutral styling
        /// </summary>
        Default = 0,

        /// <summary>
        /// Pending items - secondary color theme
        /// </summary>
        Pending = 1,

        /// <summary>
        /// In progress items - info/blue color theme
        /// </summary>
        Progress = 2,

        /// <summary>
        /// Completed items - success/green color theme
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Primary brand color theme
        /// </summary>
        Primary = 4,

        /// <summary>
        /// Secondary brand color theme
        /// </summary>
        Secondary = 5,

        /// <summary>
        /// Info/blue color theme
        /// </summary>
        Info = 6,

        /// <summary>
        /// Success/green color theme
        /// </summary>
        Success = 7
    }
}