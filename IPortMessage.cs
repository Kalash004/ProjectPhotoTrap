using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPhotoTrap
{
    public enum Type
    {
        FaceDetected,
        ServerLink
    }
    internal interface IPortMessage
    {
        public void DataRead(object sender, String message);

        public void DataRead(object sender, String message, Type type);
    }
}
