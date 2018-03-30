using System;
using System.Threading;
using System.Threading.Tasks;
using JonesCorp.Lookup;

namespace JonesCorp
{
    /*
        ///  <summary>
        ///  TODO: Change XArcadeButton Names to Button numbers as follows and remove false 
        ///  ones
        ///  ========================================================================================================
        ///  ACTUAL LAYOUT
        ///  ---------------------------------------------------------------------------------------------------------
        ///  
        /// 								                        (10:1)  (10:2)
        /// 			    (11:U)	    (1:LCTrl) (2:LAlt) (3:Space)            (11:R)      (1:A) (2:S) (3:Q)
        /// 			(13:L) (14:R)	(4:LShift)  (5:Z)    (6:X)          (13:D)  (14:G)  (4:W) (5:E) (6:[)
        /// 		        (12:D)	    (7:C)(8:5)                              (12:F)      (7:]) (8:6)      
        ///  (9:3)                                                                                          (9:4)
        ///  
        ///  
        ///  </summary>
        */

    public partial class TankStickWrapper
    {
        /// <summary>
        ///     This background worker processes the keystate of the device into xarcade buttons
        /// </summary>
        /// <param name="cancelToken"></param>
        private void BeginPoll(CancellationToken cancelToken)
        {
            
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        cancelToken.ThrowIfCancellationRequested();

                        foreach (var p in Poll().PlayerData)
                        {
                            PlayerData?.Invoke(this, p);
                        }


                        Thread.Sleep(IsRunning ? 1 : 1000);
                    }
                }
                catch (Exception xx)
                {
                    //XState = XState.Running;
                    Console.WriteLine("tankstick cancelled");
                    throw;
                }
            }, cancelToken);
        }


        /// <summary>
        ///     Manually poll the xarcade driver and return its data
        /// </summary>
        /// <returns></returns>
        public TankStickData Poll()
        {
            var data = new TankStickData();

            if (IsRunning)
            {
                var unifiedRolloverBufferCopy = new byte[14];

                // i dont want to lock the unified rollover buffer array 
                // any longer than i have to so make a copy
                lock (_unifiedRolloverBuffer)
                    Array.Copy(_unifiedRolloverBuffer, unifiedRolloverBufferCopy, _unifiedRolloverBuffer.Length);


                var snapshot = GetRollovers(unifiedRolloverBufferCopy);

                for (var i = 0; i < _cachedSnapshots.Length; i++)
                {
                    var previousKeys = _cachedSnapshots[i];
                    var currentKeys = ToSingle(snapshot, i);
                    var different = previousKeys ^ currentKeys;

                    if (different > 0)
                    {
                        data.PlayerData.Add(new PlayerEventArgs(i, currentKeys, different));
                        _cachedSnapshots[i] = currentKeys;
                    }
                }
            }

            return data;
        }
    }
}