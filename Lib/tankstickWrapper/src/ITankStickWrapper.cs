using System;
using System.Threading;
using JonesCorp.Lookup;

namespace JonesCorp
{
    public interface ItankStickWrapper : IDisposable
    {
        /// <summary>
        ///     signifies that the library is connected and running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        ///     the driver is stopped
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        ///     the driver is opened but not running
        /// </summary>
        bool IsOpened { get; }

        //bool Notify(Notified notification, String classGuid);

        /// <summary>
        ///     The current state that the library is in
        /// </summary>
        XState XState { get; }

        /// <summary>
        ///     Occurs when data arrives from the joystick
        /// </summary>
        event EventHandler<PlayerEventArgs> PlayerData;


        /// <summary>
        ///     Open all handles and claim any interfaces
        /// </summary>
        /// <returns></returns>
        bool Open();

        /// <summary>
        ///     Start Workers and receieving of data
        /// </summary>
        /// <returns></returns>
        bool Start(CancellationTokenSource tokenSource);

        /// <summary>
        ///     Stop Workers and receieving of data
        /// </summary>
        /// <returns></returns>
        bool Stop();

        /// <summary>
        ///     Close all open handles and release interfaces and resources
        /// </summary>
        /// <returns></returns>
        bool Close();
    }
}