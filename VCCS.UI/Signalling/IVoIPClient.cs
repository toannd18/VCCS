using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCCS.UI.Signalling
{
    interface IVoIPClient
    {
        void Call(string destination);
        void Cancel();
        void Answer();
        void Reject();
        void Redirect(string destination);
        void PutOnHold();
        void TakeOffHold();
        Task<bool> BlindTransfer(string destination);
        void Hangup();
        void Shutdown();
    }
}
