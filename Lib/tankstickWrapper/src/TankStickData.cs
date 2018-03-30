using System.Collections.Generic;

namespace JonesCorp
{
    /// <summary>
    ///     Data object returned when manuall polling the xarcade
    /// </summary>
    public class TankStickData
    {
        public TankStickData()
        {
            PlayerData = new List<PlayerEventArgs>();
        }

        /// <summary>
        ///     Contains data for each player
        /// </summary>
        public List<PlayerEventArgs> PlayerData { get; private set; }
    }
}