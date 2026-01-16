using System.IO.Pipes;

namespace fnpackup.Controllers
{
    public class LoggerController : BaseController
    {
        public LoggerController()
        {
            /*
            string pipeName = "/app/fnpackup.logger.debug";
            NamedPipeServerStream server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.In,
                NamedPipeServerStream.MaxAllowedServerInstances,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
            */
        }
    }


}
