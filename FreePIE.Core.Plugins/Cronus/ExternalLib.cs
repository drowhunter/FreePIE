using System;
using System.Runtime.InteropServices;

namespace FreePIE.Core.Plugins.Cronus
{
    public class ExternalLibEventArgs: EventArgs
    {
        public IntPtr handle { get; set; }
    }

    public class ExternalLib: IDisposable
    {
        protected IntPtr _pDll;
        protected string _dll;
        #region System Functions (LoadLibrary,GetProcAddress,FreeLibrary)

        [DllImport("kernel32.dll",EntryPoint="LoadLibrary")]
        private static extern IntPtr _LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll",EntryPoint="GetProcAddress")]
        private static extern IntPtr _GetProcAddress(IntPtr hModule, string procedureName);


        [DllImport("kernel32.dll",EntryPoint="FreeLibrary")]
        private static extern bool _FreeLibrary(IntPtr hModule);
        #endregion

        #region Events
        public event EventHandler<ExternalLibEventArgs> OnLoad;
        private void Fire_OnLoad(ExternalLibEventArgs e)
        {
            if (OnLoad != null) OnLoad(this, e);
        }

        public event EventHandler<ExternalLibEventArgs> OnInit;
        private void Fire_Init(ExternalLibEventArgs e)
        {
            if (OnInit != null) OnInit(this, e); 
        }

        public event EventHandler<ExternalLibEventArgs> OnClose;
        private void Fire_Close(ExternalLibEventArgs e)
        {
            if (OnClose != null) OnClose(this, e);
        }

        #endregion

        public ExternalLib(string path)
        {
            _dll = path;
            
            this.Open();
        }

        

        /// <summary>
        /// Get a Procedure from a native dll
        /// </summary>
        /// <typeparam name="T">the delegate Type to return</typeparam>
        /// <param name="p">a pointer to the dll library</param>
        /// <param name="methodname">the name of the method to retrieve</param>
        /// <returns>a delegate method for the procedure</returns>
        protected T GetProc<T>(string methodname)
        {
            Type t = typeof(T);

            IntPtr pAddressOfFunctionToCall = _GetProcAddress(_pDll, methodname);
            if (pAddressOfFunctionToCall == IntPtr.Zero)
                throw new Exception("Failed to get pointer to " + methodname);

            Delegate d = Marshal.GetDelegateForFunctionPointer(pAddressOfFunctionToCall, t);
            T method = (T)Convert.ChangeType(d, t);
            return method;
        }

        public void Open()
        {
            _pDll = _LoadLibrary(_dll);
            if (_pDll == IntPtr.Zero)
                throw new Exception("problem loading " + _dll);
            else
            {
                try
                {
                    Init();
                    
                    
                    

                }
                catch 
                {

                    this.Dispose();
                }
                finally
                {
                    
                }
            }

            
        }

        public virtual void Load()
        {
            Fire_OnLoad(new ExternalLibEventArgs
            {
                handle = _pDll
            });

           
        }

        protected virtual void Init()
        {
            Fire_Init(new ExternalLibEventArgs
            {
                handle = _pDll
            });
        }

        public virtual void Dispose()
        {
            Fire_Close(new ExternalLibEventArgs
            {
                handle = _pDll
            });

            if (_pDll != IntPtr.Zero)
            {
                _FreeLibrary(_pDll);
                _pDll = IntPtr.Zero;
            }
        }
    }
}
