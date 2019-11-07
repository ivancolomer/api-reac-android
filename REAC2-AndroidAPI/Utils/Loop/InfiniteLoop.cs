using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace REAC_AndroidApi.Utils.Loop
{
    public delegate void OnTickCallback();

    public class InfiniteLoop : IDisposable
    {
        private readonly int SleepTime;
        private OnTickCallback Callback;

        public InfiniteLoop(int SleepTime, OnTickCallback Callback)
        {
            this.SleepTime = SleepTime;
            this.Callback = Callback;

            StartLoop();
        }

        protected async void StartLoop()
        {
            while (true)
            {
                BeforeAction();

                OnTickCallback CallbackCpy = Callback;
                if (CallbackCpy != null)
                    CallbackCpy.Invoke();

                await Task.Delay(TimeToWait());
            }
        }

        protected virtual void BeforeAction()
        {
        }

        protected virtual int TimeToWait()
        {
            return SleepTime;
        }

        public void Dispose()
        {
            Callback = null;
        }
    }
}
